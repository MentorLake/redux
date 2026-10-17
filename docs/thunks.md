# Thunks

Thunks are units of asynchronous work that run against the store. Unlike [effects](effects.md), which react to actions, a thunk is *initiated* — a component, a test, or another thunk calls it and can await its result. Every thunk automatically emits standardized **lifecycle actions** (`pending`, `fulfilled`, `rejected`) that reducers and subscribers can use.

- [Why Thunks](#why-thunks)
- [Defining Thunks](#defining-thunks)
- [Lifecycle Actions](#lifecycle-actions)
- [Dispatching Thunks](#dispatching-thunks)
- [Awaiting Results](#awaiting-results)
- [Observing the Thunk Stream](#observing-the-thunk-stream)
- [Thunk Reducers](#thunk-reducers)
- [Nested Thunks](#nested-thunks)
- [Error Handling](#error-handling)
- [Cancellation](#cancellation)

## Why Thunks

Async work is where Redux apps usually become inconsistent: where does the loading flag go? Who dispatches the failure? What happens when the caller wants the result? Thunks standardize all of it:

```text
DispatchThunk(work)
   ├─► dispatches  Name/pending
   ├─► runs        await work(api)        // may dispatch any actions
   ├─► dispatches  Name/fulfilled        (with result, for ThunkFunc)
   └─► or          Name/rejected         (with exception)
```

Loading and error state become ordinary reducer logic over lifecycle actions, and callers get both an observable (`Actions`) and an awaitable (`ToTask()`).

## Defining Thunks

Thunks are values of type `ThunkAction<TApi…>` (no return value) or `ThunkFunc<TApi…, TResult>` (with return value), parameterized by the API type they need — `ThunkApi` for the default API. The first type argument is always the API; remaining type arguments are the work's parameters (up to 7).

```csharp
public static class MyThunks
{
    public static ThunkAction<ThunkApi> NoArgNoReturnThunk = new(
        "NoArgNoReturnThunk",
        async api => await Task.Delay(1000));

    public static ThunkAction<ThunkApi, int> ThunkWithArgs = new(
        "ThunkWithArgs",
        async (api, i) => await Task.Delay(i));

    public static ThunkFunc<ThunkApi, string> ThunkWithReturnValue = new(
        "ThunkWithReturnValue",
        api => Task.FromResult("Hello World"));

    public static ThunkFunc<ThunkApi, int, string> ThunkWithArgAndReturnValue = new(
        "ThunkWithArgAndReturnValue",
        (api, i) => Task.FromResult(i.ToString()));
}
```

Inside the work function, `api` gives you:

- `api.State` — the current `StoreState` (read live state, apply selectors),
- `api.Dispatch(action)` — dispatch ordinary actions into the store,
- `api.DispatchThunk(...)` — run and observe nested thunks,
- `api.Using<TApi>()` — switch to a [custom thunk API](custom-thunk-api.md).

```csharp
public static ThunkAction<ThunkApi> ThunkUsingSelector = new(
    "ThunkUsingSelector",
    api =>
    {
        var firstName = MySelectors.FirstName.Apply(api.State);
        Console.WriteLine(firstName);
        return Task.CompletedTask;
    });

public static ThunkAction<ThunkApi> ThunkDispatchingActions = new(
    "ThunkDispatchingActions",
    api =>
    {
        api.Dispatch(new MyAction());
        return Task.CompletedTask;
    });
```

### Names

Names are optional. A named thunk emits lifecycle actions prefixed with the name (`"LoadUser/pending"`); an unnamed thunk emits the same actions with an empty prefix (`"/pending"`):

```csharp
// Construct without a name
var delay = new ThunkAction<ThunkApi>(async api => await Task.Delay(100));
var load = new ThunkFunc<ThunkApi, int, string>((api, id) => Task.FromResult(id.ToString()));

// Or assign from a Func via implicit conversion
ThunkAction<ThunkApi> save = api =>
{
    api.Dispatch(new MyAction());
    return Task.CompletedTask;
};

ThunkFunc<ThunkApi, int, int> doubleIt = (api, value) => Task.FromResult(value * 2);
```

Name thunks you intend to react to in reducers or subscriptions; the name is the stable identifier. Unnamed thunks are fine for one-off fire-and-forget work.

> **Dependency injection note:** if the *thunk definitions themselves* need injected services, declare them as instance members on a class resolved from DI. For services used *inside* the work function, prefer a [custom `ThunkApi`](custom-thunk-api.md).

## Lifecycle Actions

Every thunk run emits, in order:

1. `ThunkPending` — before the work starts.
2. `ThunkFulfilled` or `ThunkFulfilled<TResult>` — after the work completes successfully.
3. `ThunkRejected` — instead of fulfilled, if the work throws. Carries the `Exception`.

All of them implement `IAction` with names derived from the thunk name:

```csharp
public record struct LifecycleNames(string actionName)
{
    public string Pending    => $"{actionName}/pending";
    public string Fulfilled  => $"{actionName}/fulfilled";
    public string Rejected   => $"{actionName}/rejected";
}
```

They are dispatched into the store like any other action, so they appear on `store.Actions` and can be reduced.

## Dispatching Thunks

`store.DispatchThunk(...)` is the entry point for the default `ThunkApi`. It accepts named wrappers, bound callables, lambdas, and method groups, with or without arguments:

```csharp
// Named wrapper (Bind() is optional for no-arg wrappers)
store.DispatchThunk(MyThunks.NoArgNoReturnThunk);
store.DispatchThunk(MyThunks.NoArgNoReturnThunk.Bind());

// Named wrapper with arguments — pass args after the wrapper, or Bind first
store.DispatchThunk(MyThunks.ThunkWithArgAndReturnValue, 123);
store.DispatchThunk(MyThunks.ThunkWithArgAndReturnValue.Bind(123));

// Inline lambda
store.DispatchThunk(async api =>
{
    api.Dispatch(new MyAction());
    await Task.Delay(100);
});

// Lambda with arguments
store.DispatchThunk(async (api, userId) =>
{
    var user = await LoadUserAsync(userId);
    api.Dispatch(new UserLoadedAction(user));
}, 42);

// Method group with arguments
store.DispatchThunk(LoadUser, 42);
```

`DispatchThunk` starts the run immediately and returns a `ThunkResult` without blocking the calling thread (see [Thunks Run Immediately](#thunks-run-immediately)). It is unrelated to `store.Dispatch` (which is synchronous and for plain actions).

### Thunks Run Immediately

Calling `DispatchThunk` starts the work right away — you do **not** need to subscribe first. The run is asynchronous: `DispatchThunk` hands back a `ThunkResult` without blocking, while the work proceeds in the background and dispatches its lifecycle actions into the store.

`ThunkResult.Actions` is a *hot, replaying* stream of that single run. You subscribe to **observe** it, not to start it:

```csharp
// The thunk is already running; this just observes it
store.DispatchThunk(MyThunks.LoadUser, 42).Actions
    .Subscribe(action => Console.WriteLine(action));
```

Because the stream replays, the timing of your subscription doesn't matter — an observer that attaches after the run has begun (or even finished) still receives the lifecycle actions it missed:

```csharp
var result = store.DispatchThunk(MyThunks.LoadUser, 42); // runs now

// ... later, after the work has already completed ...
var fulfilled = await result.Actions
    .Action<ThunkFulfilled>("LoadUser/fulfilled")
    .FirstAsync(); // still delivered, thanks to replay
```

Subscribing more than once — or calling `ToTask()` in addition to `Subscribe()` — observes the *same* run. It never re-executes the work or re-dispatches its actions. To run the work again, call `DispatchThunk` again; each call is a fresh run:

```csharp
var result = store.DispatchThunk(MyThunks.LoadUser, 42);
result.Actions.Subscribe(log);      // run #1
result.Actions.Subscribe(metrics);  // still run #1 — shared
result.ToTask();                    // still run #1 — shared

store.DispatchThunk(MyThunks.LoadUser, 42); // run #2 — a separate execution
```

Since dispatch already runs the thunk, ignoring the result is a legitimate fire-and-forget:

```csharp
// Runs in the background; nothing to await
store.DispatchThunk(MyThunks.LogEvent, payload);
```

Use `ToTask()` when you want to await completion or the return value (see below).

## Awaiting Results

`ThunkResult` converts to a `Task` when you want to await completion or a return value. The run has already started at dispatch; `ToTask()` simply observes it and completes when the thunk fulfills or rejects (see [Thunks Run Immediately](#thunks-run-immediately)):

```csharp
// Await completion (throws the rejection exception, if any)
await store.DispatchThunk(MyThunks.NoArgNoReturnThunk).ToTask();

// Await a result value
var result = await store.DispatchThunk(MyThunks.ThunkWithArgAndReturnValue, 123).ToTask();
Console.WriteLine($"Returned value: {result}");

// Inline lambda as a task
var greeting = await store.DispatchThunk(_ => Task.FromResult("Hello")).ToTask();
```

`ToTask()` completes on `fulfilled` and rethrows the original exception (preserving its stack trace) on `rejected`.

## Observing the Thunk Stream

`ThunkResult.Actions` is an observable of every action related to that thunk run: the lifecycle actions **and** any actions the work dispatches. The stream completes when the thunk fulfills or rejects.

```csharp
// Lifecycle: pending
store.DispatchThunk(MyThunks.NoArgNoReturnThunk).Actions
    .Action<ThunkPending>("NoArgNoReturnThunk/pending")
    .Subscribe(action => Console.WriteLine("Thunk started."));

// Lifecycle: fulfilled, with the result payload
store.DispatchThunk(MyThunks.ThunkWithArgAndReturnValue, 123).Actions
    .Action<ThunkFulfilled<string>>("ThunkWithArgAndReturnValue/fulfilled")
    .Subscribe(action => Console.WriteLine($"Returned value: {action.Result}"));

// Custom actions dispatched by the work
store.DispatchThunk(MyThunks.ThunkWithArgAndReturnValue, 123).Actions
    .OfType<MyAction>()
    .Subscribe(action => Console.WriteLine("My action received!"));
```

This is useful for one-shot coordination ("when this run finishes, do X") without adding global reducers. The run starts when you call `DispatchThunk`, not when you subscribe, and every subscription to the same `ThunkResult` shares that one run — so observe the result you already dispatched rather than dispatching again just to attach another observer.

## Thunk Reducers

Because lifecycle actions are ordinary actions, reducers can maintain loading/error/telemetry state with named handlers:

```csharp
public record MyThunkState(int CallCount);

public class MyThunkReducers : IReducerFactory
{
    public FeatureReducerCollection Create() =>
    [
        FeatureReducer.Build(new MyThunkState(0))
            .On<ThunkFulfilled>("NoArgNoReturnThunk/fulfilled",
                (state, action) => state with { CallCount = state.CallCount + 1 })
    ];
}
```

A typical loading/error slice:

```csharp
FeatureReducer.Build(new UserViewState(false, null, null))
    .On<ThunkPending>("LoadUser/pending",
        (state, _) => state with { IsLoading = true, Error = null })
    .On<ThunkFulfilled<User>>("LoadUser/fulfilled",
        (state, action) => state with { IsLoading = false, User = action.Result })
    .On<ThunkRejected>("LoadUser/rejected",
        (state, action) => state with { IsLoading = false, Error = action.Exception.Message });
```

## Nested Thunks

A thunk can run other thunks through `api.DispatchThunk`. Await them with `ToTask()` to sequence, or observe their streams:

```csharp
public static ThunkFunc<ThunkApi, string> ThunkDispatchingThunk = new(
    "ThunkDispatchingThunk",
    async api =>
    {
        var result = await api.DispatchThunk(MyThunks.ThunkWithReturnValue).ToTask();
        return $"from nested: {result}";
    });
```

Nested lifecycle actions flow into the store normally, so reducers see both parent and child lifecycles (`Parent/pending`, `Child/pending`, `Child/fulfilled`, `Parent/fulfilled`). If the nested thunk rejects, `ToTask()` throws inside the parent — which then rejects the parent unless you catch it.

## Error Handling

An exception thrown by the work function:

- becomes a `ThunkRejected` action carrying the exception (visible to reducers and `Actions` subscribers),
- causes `ToTask()` to rethrow the original exception,
- does **not** crash the store or affect other thunks.

Choose the channel that fits the caller:

```csharp
// Caller handles it
try
{
    await store.DispatchThunk(SaveUser, user).ToTask();
}
catch (ApiException ex)
{
    toast.Show(ex.Message);
}

// Or let reducers handle it globally via "SaveUser/rejected"
```

Prefer letting exceptions propagate to lifecycle handling over swallowing them inside the work function.

## Cancellation

Every thunk can observe `api.CancellationToken`. Pass it to async work and cancel with `Cancel()` on the `ThunkResult`, or use `DispatchThunkLatest` to cancel the previous thunk in a named group (take-latest):

```csharp
var result = store.DispatchThunk(async api =>
    await http.Search(term, api.CancellationToken));
result.Cancel();

// supersede the previous "search" thunk automatically
store.DispatchThunkLatest("search", async api =>
    await http.Search(term, api.CancellationToken));
```

A cancelled thunk dispatches `ThunkCancelled` (`{actionName}/cancelled`) instead of `fulfilled`/`rejected`, and `ToTask()` throws `OperationCanceledException`. See [Cancellation](cancellation.md) for details.

## See Also

- [Custom Thunk API](custom-thunk-api.md) — injecting services into thunks
- [Cancellation](cancellation.md) — cancellation tokens, groups, and lifecycle
- [Effects](effects.md) — reactive (non-initiated) side effects
- [Reducers](reducers.md) — handling lifecycle actions
