# Cancellation

Async work must be cancellable: search-as-you-type, route changes, and superseded refreshes all need the previous in-flight operation stopped. MentorLake.Redux provides cancellation at two levels — **thunks** (explicit, caller-driven) and **effects** (declarative Rx strategies).

- [Cancelling a Thunk](#cancelling-a-thunk)
- [Supplying Your Own Token](#supplying-your-own-token)
- [Cancellation Groups (Take-Latest Thunks)](#cancellation-groups-take-latest-thunks)
- [The Cancelled Lifecycle Action](#the-cancelled-lifecycle-action)
- [Effect Strategies](#effect-strategies)

## Cancelling a Thunk

Every thunk receives a `CancellationToken` through `ThunkApi`. Pass it to any async work, then call `Cancel()` on the `ThunkResult` returned by `DispatchThunk`:

```csharp
var result = store.DispatchThunk(async api =>
{
    await api.Http.GetAsync("/search", api.CancellationToken);
});

result.Cancel(); // signals the token
```

`ToTask()` on a cancelled thunk throws `OperationCanceledException`.

## Supplying Your Own Token

Every dispatch method accepts an optional `CancellationToken`. When you pass one, the store uses *your* token instead of creating its own — ideal when cancellation is already driven by something else (an ASP.NET request `CancellationToken`, a `CancellationTokenSource` you own, or a linked token):

```csharp
public async Task Search(CancellationToken requestAborted)
{
    await store.DispatchThunk(
        async api => await api.Http.Search(term, api.CancellationToken),
        requestAborted).ToTask();
}
```

The token flows into `ThunkApi.CancellationToken`, so the same token governs all work the thunk does.

The supplied token is *linked* with a store-owned source, so **both** cancellation paths work: cancelling your own token stops the thunk, and so does `ThunkResult.Cancel()`. The same linking applies to `DispatchThunkLatest`, where a newer thunk in the group is a third way the current one gets cancelled.

## Cancellation Groups (Take-Latest Thunks)

`DispatchThunkLatest` cancels the previously dispatched thunk in the same group — the redux-saga `takeLatest` pattern:

```csharp
// each keystroke supersedes the previous search
store.DispatchThunkLatest("search", async api =>
{
    await api.Http.Search(term, api.CancellationToken);
});
```

Groups are independent: `"search"` and `"profile"` thunks never cancel each other. A group's slot is released when its thunk finishes.

## The Cancelled Lifecycle Action

A cancelled thunk dispatches `{actionName}/cancelled` (`ThunkCancelled`) instead of `fulfilled` or `rejected`:

| Action | Meaning |
|--------|---------|
| `ThunkPending` | work started |
| `ThunkFulfilled` | completed with a result |
| `ThunkRejected` | faulted with an exception |
| `ThunkCancelled` | superseded or explicitly cancelled |

Handle it in reducers to clear loading state without showing an error:

```csharp
FeatureReducer.Build(new SearchState())
    .On<ThunkPending>(s => s with { IsLoading = true })
    .On<ThunkCancelled>(s => s with { IsLoading = false })
```

## Effect Strategies

Effects declare what happens when a new trigger arrives while work is still in flight. Build them with `Effect.Build()`:

```csharp
Effect.Build()
    .On<SearchAction>()
    .Debounce(TimeSpan.FromMilliseconds(300))
    .TakeLatest()
    .Handle(action => Observable.FromAsync(ct =>
        api.Search(((SearchAction) action).Term, ct)))
    .DispatchResults()
```

| Strategy | Behavior | redux-saga equivalent |
|----------|----------|----------------------|
| `TakeEvery()` | run all occurrences concurrently (default) | `takeEvery` |
| `TakeLatest()` | cancel in-flight work when a new trigger arrives | `takeLatest` |
| `Exhaust()` | ignore new triggers while work is in flight | `takeLeading` |

Timing operators:

| Operator | Behavior |
|----------|----------|
| `Debounce(dueTime)` | wait for a quiet period; later triggers restart the timer |
| `Throttle(window)` | emit at most one trigger per window |

### How cancellation propagates

`Handle` returns an observable per triggering action. When a strategy drops a run, the subscription is disposed, which cancels the `CancellationToken` handed to `Observable.FromAsync` — so any token-aware HTTP, database, or delay work inside stops immediately. Always flow the token through rather than ignoring it.

### Choosing between thunks and effects for cancellable work

- Caller wants a result and controls cancellation → **thunk** (`DispatchThunkLatest` or `result.Cancel()`).
- The store reacts to actions and the *pattern* (latest/leading/every) defines cancellation → **effect** with a strategy.
