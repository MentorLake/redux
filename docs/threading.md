# Threading

MentorLake.Redux is thread-safe for concurrent dispatch, but it achieves this the same way classic Redux does: by making all state mutation happen on **one thread**. Understanding that single-threaded core is the key to using the store correctly from a multi-threaded application.

- [The Single-Threaded Core](#the-single-threaded-core)
- [How the Store Serializes Work](#how-the-store-serializes-work)
- [The Dispatch Contract](#the-dispatch-contract)
- [What This Means For Your Code](#what-this-means-for-your-code)
- [See Also](#see-also)

## The Single-Threaded Core

Redux has one rule: state changes only through reducers, one action at a time. That rule is what makes state predictable — every change is a pure function applied in a known order, and the action log *is* the history of the application.

The moment two threads can run reducers simultaneously, the rule breaks. Two reducers reading the same state and writing back produce a lost update; subscribers see torn or inconsistent snapshots; the action log no longer explains the state. The original JavaScript Redux never had to defend against this because JavaScript is single-threaded. On .NET, threads are everywhere, so the store enforces the rule itself.

**All reducers, all middleware, and all state notifications run on a single dedicated processor thread.** Your code may call into the store from any thread, but the store's internals behave as if the world has one thread — because for everything that touches state, it does.

```
caller threads                processor thread
──────────────                ────────────────
Dispatch(a)  ─┐
Dispatch(b)  ─┤   channel     ┌──────────────────────────┐
Dispatch(c)  ─┼──────────────▶│ a → b → c, one at a time │
(any thread) ─┘  (thread-safe)└──────────────────────────┘
```

## How the Store Serializes Work

`Dispatch` never runs reducers on your thread. It packages the action(s) into a queue item, writes it to an unbounded channel, and waits. A single background processor reads items in FIFO order and runs the full pipeline — middleware, reducers, action stream, state notification — for one item at a time.

This gives you three guarantees:

1. **No lost updates.** Concurrent dispatches from any number of threads are applied one after another. Nothing races.
2. **Atomic batches.** A multi-action `Dispatch(a, b, c)` is a single queue item. It is applied start-to-finish with no other action interleaving, and produces exactly one state notification.
3. **A consistent action log.** The order actions reach reducers is the order they entered the queue. Subscribers and effects observe that same order.

## The Dispatch Contract

`Dispatch` is **blocking by design**. When it returns, your action has been fully processed: reducers have run, the action stream has emitted, and state notifications have fired. This mirrors classic Redux, where `dispatch` is synchronous, and it means you can always read `store.State` immediately after dispatching and see your change.

```csharp
store.Dispatch(new IncrementAction());
Console.WriteLine(store.State.GetFeatureState<CounterState>().Value); // always updated
```

The blocking wait is also what makes the store safe to use from UI frameworks: a Blazor event handler that dispatches and then re-reads state sees the new value, with no `InvokeAsync` dance or post-render timing games.

**Re-entrancy:** if you call `Dispatch` from inside middleware or a reducer (i.e., on the processor thread), the store detects this and applies the action inline instead of waiting. This is safe — you're already the single thread — and it prevents middleware that dispatches follow-up actions from deadlocking.

### What `Dispatch` does *not* wait for

The contract is **per-dispatch, not per-cascade**. When `Dispatch(action)` returns, *that* action is fully settled — reduced, emitted, notified. Actions that the pipeline re-dispatched as a consequence (an effect with `Dispatch = true` emitting follow-up actions, for example) are queued as new dispatch items and processed on the processor's schedule. They are guaranteed to arrive, but they may arrive *after* the original caller has already woken up.

```csharp
store.Dispatch(new SavePersonAction(person));
// SavePersonAction is settled: reduced, emitted, state notified.
// The effect's SavePersonCompleteAction is queued — but may not have
// been processed yet. Don't assert on it here.
```

This is deliberate. A cascade can be arbitrarily long (effect → action → effect → …), so blocking the caller until the whole graph settles would make `Dispatch` latency depend on unrelated downstream work — and would deadlock if any link in the chain ever needs the blocked thread. Classic Redux has the same property: `dispatch` returns after *your* action, and observing derived effects requires a subscription.

If you need to observe a consequence of a dispatch, subscribe to the `Actions` stream (or select the state the consequence produces) rather than assuming the cascade has drained after `Dispatch` returns. A `DispatchAndSettleAsync` primitive that waits for the full cascade is a possible future addition; until then, treat settlement of derived actions as something you await via subscription, not something `Dispatch` promises.

## What This Means For Your Code

The processor thread is the store's clock. Anything that runs *inside* the pipeline — middleware, reducers, action-stream subscribers, selector subscribers — runs on that clock, and slow or blocking code there delays every dispatch in the application. Anything that runs *outside* it — your threads calling `Dispatch`, thunks, effects — is serialized by the queue and free to take as long as it likes.

The concrete rules (what to avoid, what to prefer, and why) live in [Best Practices](best-practices.md). The short version:

- Keep middleware and reducers fast, synchronous, and pure.
- Don't block inside any subscriber; use `ObserveOn` to process state updates on your own thread.
- Put async work in thunks and effects.
- Dispatch from any thread, freely and concurrently.

## See Also

- [Best Practices](best-practices.md) — the do's and don'ts in one place
- [Slow Consumers](slow-consumers.md) — what to do when a subscriber can't keep up
- [Error Handling](error-handling.md) — how failures flow through the store
- [Batching](batching.md) — atomic multi-action dispatch
