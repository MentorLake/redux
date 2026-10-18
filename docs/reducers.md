# Reducers

Reducers define how each slice of application state responds to actions. A reducer is a pure function — given the current slice and an action, it returns the next slice.

- [Feature Reducers](#feature-reducers)
- [Handling Actions](#handling-actions)
- [Named Action Handlers](#named-action-handlers)
- [Reducer Factories](#reducer-factories)
- [Registration](#registration)
- [Immutability Patterns](#immutability-patterns)
- [Rules and Gotchas](#rules-and-gotchas)

## Feature Reducers

State is partitioned into *features*, and each feature has one `FeatureReducer<TFeatureState>` that owns its slice. You build it fluently from an initial state:

```csharp
FeatureReducer.Build(new PersonState("Hello", "World"))
    .On<UpdateFirstNameAction>((state, action) => state with { FirstName = action.FirstName })
    .On<UpdateLastNameAction>((state, action) => state with { LastName = action.LastName })
```

`FeatureReducer.Build(initialState)`:

- registers the initial value for the feature (available via `store.State.GetFeatureState<PersonState>()` and `SelectorFactory.CreateFeature<PersonState>()`),
- collects the `.On` handlers into a set of action reducers.

Each feature slice is independent: dispatching an action runs every registered handler whose action matches, and each handler only reads and writes its own slice.

## Handling Actions

`On<TAction>` matches actions by CLR type:

```csharp
FeatureReducer.Build(new CounterState(0))
    .On<IncrementAction>((state, _) => state with { Count = state.Count + 1 })
    .On<SetCountAction>((state, action) => state with { Count = action.Count })
```

If an action doesn't match any handler for a slice, that slice is returned unchanged. Handlers can also return the same instance deliberately when nothing needs to change:

```csharp
.On<SetNameAction>((state, action) =>
    state.Name == action.Name ? state : state with { Name = action.Name })
```

Returning the same instance keeps the enclosing `StoreState` reference stable for that slice, which helps selectors avoid emitting.

## Named Action Handlers

`On<TAction>(actionName, handler)` matches on the action's `ActionName` (see [Actions](actions.md)) instead of its CLR type. This is essential for reacting to thunk lifecycle actions, which all share the same few types:

```csharp
public record MyThunkState(int CallCount, bool IsLoading, string? Error);

FeatureReducer.Build(new MyThunkState(0, false, null))
    .On<ThunkPending>("LoadUser/pending",
        (state, _) => state with { IsLoading = true, Error = null })
    .On<ThunkFulfilled>("LoadUser/fulfilled",
        (state, _) => state with { IsLoading = false, CallCount = state.CallCount + 1 })
    .On<ThunkRejected>("LoadUser/rejected",
        (state, action) => state with { IsLoading = false, Error = action.Exception.Message });
```

The typed overload `On<ThunkFulfilled<string>>(...)` also works when you want the result payload:

```csharp
.On<ThunkFulfilled<string>>("GetDisplayName/fulfilled",
    (state, action) => state with { DisplayName = action.Result })
```

## Reducer Factories

Group feature reducers into an `IReducerFactory`. A factory returns a `FeatureReducerCollection` — a collection of feature reducers that can be created together:

```csharp
public class MyReducers : IReducerFactory
{
    public FeatureReducerCollection Create() =>
    [
        FeatureReducer.Build(new PersonState("Hello", "World"))
            .On<UpdateFirstNameAction>((state, action) => state with { FirstName = action.FirstName }),

        FeatureReducer.Build(new AddressState("12345"))
            .On<ZipCodeUpdatedAction>((state, action) => state with { ZipCode = action.ZipCode })
    ];
}
```

Use one factory per feature area (Person, Cart, Auth) or one per module; the store accepts any number of factories. Factories instantiated through DI can take constructor dependencies, which is handy for initial state that comes from configuration:

```csharp
public class CartReducers(AppOptions options) : IReducerFactory
{
    public FeatureReducerCollection Create() =>
    [
        FeatureReducer.Build(new CartState(options.Currency))
            .On<AddItemAction>((state, action) => state with { ItemIds = state.ItemIds.Add(action.ItemId) })
    ];
}
```

## Registration

The store exposes several overloads:

```csharp
// From factories
store.RegisterReducers(new MyReducers(), new CartReducers(options));

// From collections directly
FeatureReducerCollection reducers = new MyReducers().Create();
store.RegisterReducers(reducers);

// From individual feature reducers
store.RegisterReducers(
    FeatureReducer.Build(new CounterState(0))
        .On<IncrementAction>((state, _) => state with { Count = state.Count + 1 }));

// Auto-discover from an assembly (parameterless constructors only)
store.RegisterAssembly(typeof(MyReducers).Assembly);
```

Registration initializes feature state immediately and notifies state subscribers. Register all reducers **before** dispatching; reducers registered later only affect subsequent dispatches.

## Immutability Patterns

Reducers must not mutate the incoming state. The idiomatic tools are record `with` expressions and immutable collections:

```csharp
public record CartState(ImmutableList<string> ItemIds, decimal Total);

FeatureReducer.Build(new CartState(ImmutableList<string>.Empty, 0m))
    .On<AddItemAction>((state, action) => state with
    {
        ItemIds = state.ItemIds.Add(action.ItemId),
        Total = state.Total + action.Price
    })
    .On<ClearCartAction>((state, _) => state with { ItemIds = ImmutableList<string>.Empty, Total = 0m })
```

For entity collections, prefer the normalized [`DataTable<TKey, TValue>`](datatable.md), which is purpose-built for immutable upsert/remove semantics:

```csharp
public record UsersState(DataTable<int, User> Users);

FeatureReducer.Build(new UsersState(new DataTable<int, User>()))
    .On<UsersLoadedAction>((state, action) => state with
    {
        Users = state.Users.UpsertMany(action.Users)
    })
```

## Rules and Gotchas

1. **Handlers must be pure.** No HTTP, no file access, no `DateTime.Now`, no captured mutable state. Effects and thunks exist for anything impure.
2. **All matching reducers run.** If two features handle the same action, both update — this is a feature, not a bug, but be deliberate about it.
3. **Return new instances when data changes.** Because change detection is reference/value based, mutating a list in place means selectors will never notice.
4. **Feature state types are keys.** Two features cannot share the same CLR state type; give each slice its own record type.
5. **Initial state is registered once.** Re-registering a reducer resets that feature's state.

Handlers run on the store's processor thread, so keep them fast — see [Threading](threading.md) for why slow code there delays every dispatch.

## See Also

- [Actions](actions.md) — what reducers respond to
- [Selectors](selectors.md) — reading the state reducers produce
- [Thunks](thunks.md) — dispatching lifecycle actions reducers can handle
