# Custom Thunk API

Thunks receive a `ThunkApi` object that provides `State`, `Dispatch`, `DispatchThunk`, and `Using`. When your thunks need application services — HTTP clients, repositories, navigation, dialogs — subclass `ThunkApi`, put those services on it, and dispatch thunks through that custom API type.

- [Why a Custom API](#why-a-custom-api)
- [Subclassing ThunkApi](#subclassing-thunkapi)
- [Registering the Factory](#registering-the-factory)
- [Dispatching Through a Custom API](#dispatching-through-a-custom-api)
- [Defining Thunks Against the Custom API](#defining-thunks-against-the-custom-api)
- [Naming Ad-Hoc Work](#naming-ad-hoc-work)
- [Nested Dispatch with Custom APIs](#nested-dispatch-with-custom-apis)
- [Design Notes](#design-notes)

## Why a Custom API

The default `ThunkApi` knows only about the store. Thunks that call services have three bad alternatives: capturing static singletons, closing over services from wherever the thunk was defined, or making every thunk definition a DI-resolved instance. A custom `ThunkApi` solves this cleanly:

- Thunks stay as plain static functions — easy to find, compose, and test.
- Services arrive through a **typed, explicit** dependency (`api.Users`), visible in the thunk body.
- Tests can construct the API with fake services directly.

## Subclassing ThunkApi

A subclass takes a `ThunkApiContext` (the library-supplied plumbing) plus whatever services it needs, and passes the context to the base constructor:

```csharp
public class AppThunkApi : ThunkApi
{
    public AppThunkApi(ThunkApiContext context, IUserService users) : base(context)
    {
        Users = users;
    }

    public IUserService Users { get; }
}
```

You can also add helper methods that all thunks share:

```csharp
public class AppThunkApi : ThunkApi
{
    public AppThunkApi(ThunkApiContext context, IUserService users, INavigator navigator)
        : base(context)
    {
        Users = users;
        Navigator = navigator;
    }

    public IUserService Users { get; }
    public INavigator Navigator { get; }

    public Task<User> LoadUserAsync(int id) => Users.GetAsync(id);
}
```

## Registering the Factory

The store creates API instances through a registered factory. Register one per custom API type before dispatching through it:

```csharp
store.RegisterThunkApiFactory(ctx => new AppThunkApi(ctx, userService));
```

With DI, resolve the services inside the lambda:

```csharp
store.RegisterThunkApiFactory(ctx =>
    new AppThunkApi(
        ctx,
        serviceProvider.GetRequiredService<IUserService>(),
        serviceProvider.GetRequiredService<INavigator>()));
```

Dispatching through an API type with no registered factory throws `InvalidOperationException` naming the missing type — the failure is immediate and explicit.

## Dispatching Through a Custom API

`store.Using<TApi>()` returns a `ThunkDispatcher<TApi>` that mirrors `store.DispatchThunk` for your API type:

```csharp
await store.Using<AppThunkApi>().Dispatch(AppThunks.Refresh).ToTask();
var name = await store.Using<AppThunkApi>().Dispatch(AppThunks.GetDisplayName, 42).ToTask();
```

Cache the dispatcher if you use it frequently:

```csharp
var thunks = store.Using<AppThunkApi>();

await thunks.Dispatch(async api =>
{
    var user = await api.Users.GetAsync(1);
    api.Dispatch(new UserLoadedAction(user));
}).ToTask();
```

`store.DispatchThunk(...)` remains sugar for `store.Using<ThunkApi>().Dispatch(...)` — the default API needs no registration.

## Defining Thunks Against the Custom API

Any thunk shape works with a custom API type — method groups, lambdas, and named wrappers:

```csharp
public static class AppThunks
{
    // Plain methods: dispatch as method groups
    public static async Task LoadUser(AppThunkApi api)
    {
        var user = await api.Users.GetAsync(1);
        api.Dispatch(new UserLoadedAction(user));
    }

    public static Task<string> GetDisplayName(AppThunkApi api, int id) =>
        api.Users.GetDisplayNameAsync(id);

    // Named wrappers behave exactly as with the default API
    public static ThunkAction<AppThunkApi> Refresh = new(
        "Refresh",
        async api => await api.Users.RefreshAsync());

    public static ThunkFunc<AppThunkApi, int, User> LoadUserById = new(
        "LoadUserById",
        async (api, id) => await api.Users.GetAsync(id));
}
```

Dispatch them the same way:

```csharp
// One-off, via method group
await store.Using<AppThunkApi>().Dispatch(AppThunks.LoadUser).ToTask();

// With arguments
var name = await store.Using<AppThunkApi>().Dispatch(AppThunks.GetDisplayName, 42).ToTask();

// Named wrapper
await store.Using<AppThunkApi>().Dispatch(AppThunks.Refresh).ToTask();
```

## Naming Ad-Hoc Work

Lambdas and method groups dispatched directly have no name, so their lifecycle actions have empty prefixes. Pass an optional name as the first argument to `Dispatch` when you want lifecycle actions to be addressable (wrappers like `ThunkAction<TApi>` already carry their name):

```csharp
await store.Using<AppThunkApi>().Dispatch("LoadUser", AppThunks.LoadUser).ToTask();

var name = await store.Using<AppThunkApi>()
    .Dispatch("GetDisplayName", AppThunks.GetDisplayName, 42)
    .ToTask();
```

These now emit `LoadUser/pending`, `LoadUser/fulfilled`, etc., and can be matched by reducers with `On<ThunkFulfilled>("LoadUser/fulfilled", ...)` or by subscribers with `Action<ThunkFulfilled>("LoadUser/fulfilled")`.

## Nested Dispatch with Custom APIs

`ThunkApi` itself exposes `Using<TApi>()`, so a thunk running on one API can invoke thunks written against another:

```csharp
await store.Using<AppThunkApi>().Dispatch(async api =>
{
    await api.Using<AppThunkApi>().Dispatch(async inner =>
    {
        await inner.Users.RefreshAsync();
    }).ToTask();
}).ToTask();
```

This lets you layer APIs — for example, a base `AppThunkApi` with core services and a feature-specific `AdminThunkApi` with additional ones — without forcing every thunk onto the largest API.

## Design Notes

- **APIs are cheap.** A new API instance is created per thunk run by your factory; keep construction light and let the factory pull singletons from DI.
- **One API per dependency profile.** Rather than one god-API, define APIs per application area; thunks declare exactly what they need via their `TApi` parameter.
- **Testability.** Because thunks are static functions over a typed API, unit tests construct the API with fakes and invoke the thunk directly — no store required for pure logic, or a real store for lifecycle assertions (see [Testing](testing.md)).
- **`ThunkApiContext` is opaque.** It carries the store and the run's internal action dispatcher; subclasses pass it through and don't otherwise interact with it.

## See Also

- [Thunks](thunks.md) — the core thunk model
- [Testing](testing.md) — testing thunks with fake APIs
