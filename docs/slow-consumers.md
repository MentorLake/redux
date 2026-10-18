# Slow Consumers

A selector subscription is a callback on the store's processor thread. When the subscriber is slower than the dispatch rate — rendering, calling an API, writing to disk — updates arrive faster than they can be handled. This document explains why that happens and the Rx patterns for dealing with it.

- [The Problem](#the-problem)
- [Choosing a Strategy](#choosing-a-strategy)
- [The Patterns](#the-patterns)
- [Notes and Pitfalls](#notes-and-pitfalls)

## The Problem

The store processes actions on a single thread and pushes state notifications synchronously to subscribers. Fast dispatch means fast notifications. If your handler takes 200 ms and the store dispatches every 10 ms, the handler falls behind — and *how* it falls behind is determined by what you put between the selector and the `Subscribe`:

- **Nothing:** the handler runs on the processor thread. A slow handler *is* slow dispatch — the entire store queues behind it. Never acceptable for nontrivial work.
- **`ObserveOn`:** the processor is released immediately, and updates queue on the target scheduler. Correct first step — but the queue is unbounded, and under sustained overload you accumulate work (and memory) processing states that were obsolete the moment they were queued.

The fundamental question every slow consumer must answer:

> When three updates arrive while you're busy with the first, what should happen to the second and third?

Keep them all? Keep only the newest? Cancel the first and jump to the newest? That answer selects your pattern.

## Choosing a Strategy

| Contract | Pattern |
|---|---|
| Every value, serial, async handler | `Select(FromAsync).Concat()` |
| Every value, serial, per-item cancellation | `Concat()` + `TakeUntil` |
| Every value, serial, sync handler | `ObserveOn(ctx).Synchronize()` |
| Every value, serial, dedicated thread / thread affinity | `ObserveOn(EventLoopScheduler)` |
| Newest wins, cancel current work | `Select(FromAsync).Switch()` |
| Newest per frame/clock | `Sample(TimeSpan)` |
| Newest on external trigger | `Sample(triggerObservable)` |
| Newest, event-driven, no clock | `ConflateLatest(scheduler)` |
| Always-current value, pull model | `Publish` + `BehaviorSubject` |

Rule of thumb:

- The handler must see **every** state (audit log, persistence, event sourcing) → the `Concat` family.
- The handler only cares about **what's true now** (rendering, autosave, syncing to a service) → `Switch` or `Sample`.
- Processing a stale value is **actively wasteful** (fetch that's already obsolete) → `Switch`.
- The consumer is **frame-driven** → `Sample`.
- The consumer is **busy-driven** — emit the newest value as soon as it's free, no clock involved → `ConflateLatest`.

## The Patterns

All examples assume the selector already applies `DistinctUntilChanged` (every `store.Select` does), so only genuinely changed values reach the queue.

### Every value, serially: `Concat`

The canonical task-based queue. Each handler runs to completion before the next value starts; values arriving meanwhile wait in order. Push-driven — no polling anywhere.

```csharp
store.Select(s => s.GetFeatureState<CartState>())
    .Select(cart => Observable.FromAsync(ct => SaveCartAsync(cart, ct)))
    .Concat()
    .Subscribe();
```

- **Guarantees:** serial, in-order, nothing dropped, cancellation via the token on dispose.
- **Cost:** unbounded queue under sustained overload. Every stale state is still processed.

The shortest forms, depending on what your handler looks like:

```csharp
.Select(x => Observable.FromAsync(ct => HandleAsync(x, ct)))  // token-aware
.Select(x => Observable.FromAsync(() => HandleAsync(x)))       // no token
.Select(x => Observable.Start(() => HandleSync(x)))            // sync handler
```

### Every value, serially, with per-item cancellation

Plain `Concat` never interrupts an in-flight item; `Switch` always interrupts it. This middle ground lets an **event** cancel the current item while keeping the serial queue intact — the abandoned item simply doesn't finish, and the next queued value proceeds.

```csharp
store.Select(s => s.GetFeatureState<DraftState>())
    .Select(draft => Observable.FromAsync(ct => SaveAsync(draft, ct))
        .TakeUntil(store.Actions.OfType<DiscardDraftAction>()))
    .Concat()
    .Subscribe();
```

`TakeUntil` completes the inner sequence when the signal fires; `Concat` treats completion as "done" and advances. The cancellation is per-item and event-driven, not arrival-driven.

### Every value, serially, sync handler: `ObserveOn` + `Synchronize`

`Synchronize` guarantees the handler is never entered concurrently — but on its own it blocks the *producer*, which here is the store. Pair it with `ObserveOn` so the producer being blocked is only the scheduler's pump, never the processor:

```csharp
store.Select(MySelectors.User)
    .ObserveOn(mySyncContext)
    .Synchronize()
    .Subscribe(Handle);
```

Every value is delivered, one at a time, with no task plumbing in the handler.

### Every value, serially, dedicated thread: `EventLoopScheduler`

`EventLoopScheduler` is a single dedicated thread with an internal FIFO queue — serialization by construction, so `Synchronize` is unnecessary, and the handler always runs on the *same* thread (useful for thread-affine consumers):

```csharp
var scheduler = new EventLoopScheduler(
    t => new Thread(t) { Name = "redux-consumer", IsBackground = true });

store.Select(MySelectors.User)
    .ObserveOn(scheduler)
    .Subscribe(Handle);
```

Dispose the subscription *and* the scheduler when done.

### Newest wins, cancel current work: `Switch`

When processing an obsolete value is worthless, let the next arrival kill the in-flight work:

```csharp
store.Select(MySelectors.SearchTerm)
    .Select(term => Observable.FromAsync(ct => SearchAsync(term, ct)))
    .Switch()
    .Subscribe(RenderResults);
```

The token is canceled on switch, so the handler must honor it. This is the search-as-you-type pattern.

### Newest per clock: `Sample`

Deliver the latest value on a fixed cadence, discarding everything between ticks — the frame-rate pattern for rendering:

```csharp
store.Select(MySelectors.Layout)
    .Sample(TimeSpan.FromMilliseconds(32))
    .ObserveOnDispatcher()
    .Subscribe(Render);
```

A `Sample` overload also takes a **trigger observable** instead of a timespan: the value is emitted whenever the trigger fires (a frame tick, a batch-commit signal, a manual `Unit` push).

### Newest, event-driven: `ConflateLatest`

`Sample` needs a clock. `ConflateLatest` (provided by `MentorLake.Redux`) needs only the consumer itself: it hands each value to a scheduler, and while that work is in flight, new values overwrite a single pending slot instead of queueing. When the work finishes, the *newest* pending value goes out — or nothing does, if none arrived.

```csharp
using MentorLake.Redux;

store.Select(MySelectors.Metrics)
    .ConflateLatest(scheduler)
    .Subscribe(Report);
```

- **Guarantees:** at most one emission in flight at a time; the newest value always wins; nothing is lost mid-race; errors and completion pass through; disposal stops emissions immediately.
- **Cost:** like `Switch`, intermediate values are dropped — but unlike `Switch`, in-flight work is *never canceled*. The current emission runs to completion; only queued-but-unstarted values are collapsed.
- **Scheduler choice matters:** `ThreadPoolScheduler.Instance` collapses a burst of dispatches into a few reports on pool threads; an `EventLoopScheduler` gives the same conflation with thread affinity. A synchronous/immediate scheduler degenerates to pass-through.

Pick it over `Sample` when there is no natural cadence — the consumer's own throughput *is* the cadence. Pick it over `Switch` when the work can't or shouldn't be canceled mid-flight (a synchronous send, a fire-and-forget log line).

### Always-current value, pull model: `BehaviorSubject`

If the consumer shouldn't be notified at all — it just wants "whatever is current" whenever it's ready — keep the latest in a cell and let the consumer read it:

```csharp
var latest = store.Select(MySelectors.Metrics)
    .Publish(new BehaviorSubject<Metrics>(Metrics.Empty));
latest.Connect();

// consumer loop reads latest.Value on its own schedule
```

No events, no queue: the value is always fresh because it's overwritten in place.

## Notes and Pitfalls

### Keep the queue alive

In `Concat`/`Switch` chains, a faulted inner task terminates the entire subscription — future values silently stop being processed. Catch inside the inner sequence so one failure doesn't kill the queue:

```csharp
.Select(x => Observable.FromAsync(ct => HandleAsync(x, ct))
    .Catch((Exception ex) =>
    {
        logger.LogError(ex, "handler failed");
        return Observable.Empty<Unit>();
    }))
.Concat()
.Subscribe();
```

### `Synchronize` alone is not a solution

`Synchronize` prevents re-entrancy by blocking the producer. Without `ObserveOn` upstream, the producer is the store's processor thread — you've converted a slow subscriber into a stalled store. It only becomes a solution when paired with a hand-off.

### Unbounded queues are a silent memory leak

`Concat`, `ObserveOn`, `Synchronize`, and `EventLoopScheduler` all queue every pending value. If the producer is *persistently* faster than the consumer, memory grows until something breaks. If that's possible in your app, choose a bounded strategy (`Switch`, `Sample`, `ConflateLatest`) rather than hoping the burst passes.

### Don't poll

None of these patterns require timers except `Sample`, and there the timer *is* the consumer's cadence, not a polling loop. Reaching for `while (true) { read state; delay; }` means one of the patterns above fits instead.

### `DistinctUntilChanged` is already doing coalescing

Selector results only emit when they actually change. A burst of dispatches that doesn't alter the selected slice never reaches your queue at all. The patterns here handle the remaining case: values that genuinely change faster than you can consume them.

## See Also

- [Threading](threading.md) — why the processor thread must never block
- [Best Practices](best-practices.md) — the do's and don'ts
- [Reactive Programming](reactive-programming.md) — the Rx foundations
- [Selectors](selectors.md) — `DistinctUntilChanged` and derived state
