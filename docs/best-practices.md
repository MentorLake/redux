# Best Practices

Practical guidance for working with MentorLake.Redux safely and idiomatically. The rules fall out of two properties of the store: everything that touches state runs on a **single processor thread** (see [Threading](threading.md)), and failures route to the **dispatching caller and the `Errors` stream** (see [Error Handling](error-handling.md)).

- [Do](#do)
- [Don't](#dont)

## Do

### Dispatch from anywhere

Concurrent `Dispatch` calls from thread pools, UI threads, timers, and parallel loops are fully supported. No locks, no `Invoke`, no marshalling needed.

```csharp
Parallel.For(0, 8, _ =>
{
    for (var i = 0; i < 1000; i++)
        store.Dispatch(new IncrementAction());
});
// 8000 — every update applied, none lost
```

### Batch related actions into one dispatch

One logical update should be one queue item. This gives subscribers a single coherent notification and keeps batches atomic relative to other threads:

```csharp
store.Dispatch(
    new SetUserAction(user),
    new SetPreferencesAction(prefs),
    new SetCartAction(cart));
```

See [Batching](batching.md).

### Keep middleware synchronous and fast

Treat middleware like an interrupt handler: inspect, transform, guard, log — then `next`. Anything that could take unbounded time (network, disk, `Task.Delay`) belongs in a thunk or effect, which runs outside the processor.

### Put async work in thunks and effects

Thunks and effects run on their own schedulers. They dispatch actions back into the store, which the processor serializes like any other. This is the intended pattern for I/O:

```csharp
// runs off the processor; its dispatches are queued like everyone else
public ThunkAction<ThunkApi> LoadUser = new("LoadUser", async api =>
{
    var user = await _service.GetUser();      // slow, but the store keeps moving
    api.Dispatch(new UserLoadedAction(user)); // queued, applied in order
});
```

### Observe state with selectors, not polling

Selectors are pushed through the state observable on the processor thread. Subscribing to a selector is the thread-safe way to react to change; polling `store.State` from a timer is not (you can observe intermediate states between batches).

### Use `ObserveOn` to process state updates on your own thread

The ideal pattern for anything nontrivial — UI updates, I/O, aggregation — is to marshal off the processor thread immediately with `ObserveOn`, so your handler runs on a scheduler of your choosing and the processor is released without waiting for you:

```csharp
// UI framework: deliver on the dispatcher
store.Select(MySelectors.User)
    .ObserveOnDispatcher()
    .Subscribe(user => Render(user));

// No UI context: deliver on the thread pool
store.Select(MySelectors.Report)
    .ObserveOn(TaskPoolScheduler.Default)
    .Subscribe(report => _service.Upload(report));
```

The selector itself still evaluates on the processor thread (cheap and pure), but everything downstream of `ObserveOn` runs on your thread. Without it, every subscriber is implicitly a middleware: slow code there delays every dispatch in the application.

If your subscriber is slower than the dispatch rate, `ObserveOn` alone just moves the queue — see [Slow Consumers](slow-consumers.md) for the full set of strategies (`Concat`, `Switch`, `Sample`).

### Write reducers as pure functions

Reducers are `(state, action) => newState`. Same inputs, same output, no I/O, no ambient state, no mutation. Return updated copies (`with` expressions, new collections) so the store's reference-comparison change detection works.

### Model expected failures as actions

An API 404 or a validation error is application state, not an exception. Dispatch `LoadFailedAction` and render the failure. Reserve exceptions for bugs and invariant violations — they route to the caller and `Errors`, not to the UI. See [Error Handling](error-handling.md).

### Subscribe to `Errors` once at startup

```csharp
store.Errors.Subscribe(ex => logger.LogError(ex, "Redux failure"));
```

Blocking callers always see their own exceptions, but fire-and-forget dispatches (effects, background tasks) have nobody listening. The `Errors` subscription is the application-wide safety net.

### Register everything before you start dispatching

`RegisterReducers`, `UseMiddleware`, and `RegisterEffects` are intended for startup wiring. They mutate store configuration; doing it while actions flow is technically possible but invites ordering surprises. Wire first, dispatch second.

### Dispose when done

`await store.DisposeAsync()` drains everything already queued, waits for the processor to finish, and releases the streams. Pending dispatches complete normally; new ones throw `ObjectDisposedException`.

## Don't

### Don't block in middleware or reducers

The processor thread is shared by every dispatch in the application. A middleware that does `Thread.Sleep`, a synchronous HTTP call, or any other blocking I/O stalls **every** dispatch from **every** thread until it finishes.

```csharp
// BAD — blocks the entire store for the duration of the call
public void Invoke(object action, NextMiddleware next)
{
    _httpClient.Post("/log", action); // sync-over-HTTP: everything queues behind it
    next(action);
}
```

### Don't use blocking operators on selector subscriptions

Selector notifications run on the processor thread, so blocking inside a subscriber stalls the entire store — and blocking operators are the sneaky version of this. `Last()`, `First()`, `Single()`, `ForEach()`, or a `Subscribe` handler that calls `.Wait()`/`.Result` all block a thread waiting for the store to emit:

```csharp
// BAD — blocks waiting for the processor thread to produce more state
var final = store.Select(MySelectors.User).Last();

// BAD — blocks the processor thread inside the notification itself
store.Select(MySelectors.Count).Subscribe(_ =>
    _httpClient.Post("/sync", store.State)); // sync-over-HTTP on the processor thread
```

State observables are infinite hot streams — `Last()` on one never returns until the store is disposed. Treat every selector subscription as code running on the store's clock: keep it non-blocking, or hop off with `ObserveOn` first.

### Don't switch threads inside middleware

Middleware runs on the processor thread. If you `await` inside an async middleware and resume on a different thread (the default), the re-entrancy guard no longer recognizes you as "in processing" — a `Dispatch` after the resume blocks a pool thread waiting for the processor instead of taking the no-wait path.

```csharp
// BAD — resumes on a pool thread, then dispatches as an outsider
ReduxMiddleware.Create(async (action, next) =>
{
    next(action);
    await Task.Delay(10);              // thread switch
    store.Dispatch(new FollowUpAction()); // blocks this pool thread until processed
});
```

This isn't a deadlock, but it wastes a thread and defeats the no-wait fast path. Middleware is a synchronous pipeline stage by contract. If you need async work, dispatch an action that starts a thunk.

### Don't capture caller-thread state in reducers

Reducers must be pure functions of `(state, action)`. They must not read thread-locals, `AsyncLocal`, `HttpContext`, or any ambient state — they run on the processor thread, not yours.

### Don't mutate state in place

Reducers replace state (`with` expressions, new collections). The store detects "did this dispatch change anything" by **reference comparison**. A reducer that mutates and returns the same instance will silently suppress notifications.

### Don't hold references to old state and assume it's current

`store.State` is a snapshot property. Between reading it and acting on it, other threads may have dispatched. Always read state at the moment of use, or better, use selectors.

### Don't dispatch from a fire-and-forget task without an `Errors` subscription

Dispatch failures surface to the dispatching caller. A fire-and-forget `Task.Run(() => store.Dispatch(...))` has nobody listening — subscribe to `store.Errors` to catch those (see [Error Handling](error-handling.md)).

### Don't swallow exceptions in middleware

```csharp
// BAD — hides failures from both the caller and the Errors stream
ReduxMiddleware.Create((action, next) =>
{
    try { next(action); }
    catch { }
});
```

If you intentionally tolerate an action failure, log it and rethrow — don't make it invisible.

### Don't call `DisposeAsync` and `Dispatch` in a race and expect the dispatch to land

Once disposal begins, queued actions are drained, but new dispatches throw `ObjectDisposedException`. Treat disposal as "no more dispatches from now on."

## See Also

- [Threading](threading.md) — why the store behaves this way
- [Blazor View Models](blazor-viewmodels.md) — the component subscription pattern
- [Slow Consumers](slow-consumers.md) — backpressure patterns for slow subscribers
- [Error Handling](error-handling.md) — the failure model in detail
- [Batching](batching.md) — atomic multi-action dispatch
- [Middleware](middleware.md) — the synchronous pipeline stage
