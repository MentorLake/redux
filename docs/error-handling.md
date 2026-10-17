# Error Handling

Exceptions in MentorLake.Redux follow one principle: **the caller of `Dispatch` owns the failure of that dispatch; everything else goes to the `Errors` stream.** The store itself never dies from a bad action.

- [The Two Channels of Failure](#the-two-channels-of-failure)
- [Dispatch Failures](#dispatch-failures)
- [The Errors Stream](#the-errors-stream)
- [Batch Semantics](#batch-semantics)
- [What the Store Guarantees](#what-the-store-guarantees)
- [Patterns](#patterns)

## The Two Channels of Failure

A dispatch can fail in middleware or in a reducer. The store routes every failure to exactly the right audience:

| Where you are | How you learn about the failure |
|---|---|
| The thread that called `Dispatch` | The exception is rethrown to you (the call blocks until processing completes) |
| Anyone else | `store.Errors` — an `IObservable<Exception>` of every dispatch failure |

This split means blocking callers always get precise, per-dispatch error handling, while fire-and-forget paths (effects, background tasks) are never silent — as long as something subscribes to `Errors`.

## Dispatch Failures

`Dispatch` is synchronous from the caller's perspective, so failures are ordinary exceptions:

```csharp
try
{
    store.Dispatch(new PlaceOrderAction(cart));
}
catch (InvalidOperationException ex)
{
    // middleware or reducer rejected this action
}
```

Details:

- The exception thrown is the original exception from your middleware or reducer — no wrapper types.
- A failure **does not** corrupt the store. The processor catches it, finishes the current item, and moves to the next queued item. One bad action can never wedge or kill the store.
- After a failure, `store.State` reflects every reduction that succeeded before the throw (see [Batch Semantics](#batch-semantics)).

## The Errors Stream

```csharp
store.Errors.Subscribe(ex =>
{
    _logger.LogError(ex, "Redux dispatch failed");
    _telemetry.TrackException(ex);
});
```

- Fires for **every** dispatch failure, including ones a blocking caller also sees. If you both block and subscribe, you'll observe the same exception twice — that's intentional: `Errors` is the application-wide audit trail, the `catch` block is the local reaction.
- Fires on the processor thread. Keep handlers fast and non-blocking; a slow `Errors` subscriber stalls the store just like slow middleware would (see [Threading](threading.md)).
- If nothing subscribes, failures in fire-and-forget dispatches are lost. **Every application should subscribe once at startup.**
- The stream completes when the store is disposed.

## Batch Semantics

A multi-action dispatch is one atomic queue item, and failure inside it follows clear rules:

```csharp
store.Dispatch(actionA, boom, actionC);
// actionA: reduced, applied
// boom:    middleware throws
// actionC: never runs
```

1. **Actions before the failure are kept.** Reductions already applied are legitimate state transitions; the store does not roll them back. (Middleware side effects can't be undone either — rollback would be a lie.)
2. **Actions after the failure are skipped.** The batch stops at the throwing action.
3. **Subscribers are notified if state changed.** If `actionA` changed state, one state notification fires *before* the caller sees the exception. Subscribers never sit on stale state while `store.State` has moved.
4. **If nothing changed yet, nothing is notified.** A failure on the first action of a batch produces no notification.
5. **The caller's exception is the throwing action's exception**, and the `Errors` stream receives it too.

Design consequence: if a group of actions must be all-or-nothing, make them fail (or not dispatch) **before** the first one mutates — e.g. validate in the first middleware or in a guard action. Once reductions start, partial application is the contract.

## What the Store Guarantees

- **Containment:** an exception in middleware, a reducer, or the action stream is caught per queue item. The processor loop survives; subsequent dispatches work normally.
- **Consistency:** state notifications and `store.State` never diverge, even on failure.
- **No silent loss:** every failure reaches the blocking caller and the `Errors` stream.
- **Clean shutdown:** `DisposeAsync` drains the queue and completes; dispatches after disposal throw `ObjectDisposedException` rather than hanging.

The one acknowledged gap: if the processor loop itself were killed by an unrecoverable failure (not an action failure — a runtime-level fault), in-flight dispatchers would wait forever. This is treated as a process-level crash scenario, not something to program against.

## Patterns

### Subscribe once, at startup

```csharp
// in app composition root
store.Errors.Subscribe(ex => logger.LogError(ex, "Redux failure"));
```

### Guard with middleware, fail fast

Put invariants in middleware that throws *before* calling `next`, so nothing reduces against invalid state:

```csharp
ReduxMiddleware.Create((action, next) =>
{
    if (action is RequiresAuthAction && !_authService.IsAuthenticated)
        throw new InvalidOperationException("Not authenticated");
    next(action);
});
```

Because the guard runs before `next`, no state has changed when it throws — the batch is cleanly rejected.

### Prefer thunks for expected failures

An exception is for the unexpected. Expected failures (API returned 404, validation failed) should flow as *actions*, not exceptions:

```csharp
public ThunkAction<ThunkApi> LoadUser = new("LoadUser", async api =>
{
    try { api.Dispatch(new UserLoadedAction(await _service.GetUser())); }
    catch (HttpRequestException) { api.Dispatch(new UserLoadFailedAction()); }
});
```

Reducers then render the failure state. Reserve the exception paths for bugs and invariant violations.

### Don't swallow in middleware

```csharp
// BAD — hides failures from both channels
ReduxMiddleware.Create((action, next) =>
{
    try { next(action); }
    catch { }
});
```

If you intentionally tolerate an action failure, log it and rethrow, or publish to `Errors` yourself — don't make it invisible.

### Test failures like any other behavior

```csharp
Assert.Throws<InvalidOperationException>(
    () => store.Dispatch(new PlaceOrderAction(emptyCart)));
Assert.AreEqual(0, store.State.GetFeatureState<CartState>().Total);
```

See [Testing](testing.md).
