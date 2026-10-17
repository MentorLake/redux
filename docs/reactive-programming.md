# Reactive Programming in MentorLake.Redux

MentorLake.Redux is built on [System.Reactive](https://github.com/dotnet/reactive) (Rx). Actions and state are exposed as `IObservable<T>`, and effects are Rx pipelines. You don't need to be an Rx expert to use the library, but a working knowledge of a handful of operators is what turns it from "a state container" into "a event-driven application platform".

- [IObservable in 60 Seconds](#iobservable-in-60-seconds)
- [Where Rx Appears in the Library](#where-rx-appears-in-the-library)
- [Hot Streams and Subscriptions](#hot-streams-and-subscriptions)
- [DistinctUntilChanged: The Library's Core Guarantee](#distinctuntilchanged-the-librarys-core-guarantee)
- [The Operators You Actually Need](#the-operators-you-actually-need)
- [Bridging Tasks and Observables](#bridging-tasks-and-observables)
- [Disposal](#disposal)
- [Threading](#threading)

## IObservable in 60 Seconds

`IObservable<T>` is the push-based dual of `IEnumerable<T>`: instead of the consumer pulling items, the producer pushes them.

```csharp
IObservable<string> observable = ...;

IDisposable subscription = observable
    .Subscribe(
        onNext: value => Console.WriteLine(value),
        onError: ex => Console.WriteLine($"failed: {ex}"),
        onCompleted: () => Console.WriteLine("done"));
```

Key ideas:

- **Subscribe, then receive.** Nothing happens until you subscribe.
- **`Subscribe` returns an `IDisposable`.** Disposing stops delivery — this is your teardown handle.
- **A stream ends** with either `OnCompleted` or `OnError`, never both.
- **Operators return new observables.** Pipelines are declarations; each subscription activates one.

## Where Rx Appears in the Library

| Surface | Type | Meaning |
|---------|------|---------|
| `store.Actions` | `IObservable<object>` | Every action dispatched, in order (hot broadcast) |
| `store.Select(...)` | `IObservable<T>` | Current + future values of a projection, deduplicated |
| `ISelector<T>.Apply(IObservable<StoreState>)` | `IObservable<T>` | A selector lifted over a state stream |
| `Effect` | `IObservable<object> → IObservable<object>` | A pipeline from the action stream to side effects/actions |
| `ThunkResult.Actions` | `IObservable<object>` | All actions from one thunk run, completing at fulfilled/rejected |

The store itself uses a `BehaviorSubject` for state (emits the latest state to each new subscriber) and a `Subject` for actions (emits only to subscribers at dispatch time).

## Hot Streams and Subscriptions

`store.Actions` is a **hot** stream: actions flow whether or not anyone is listening, and a subscriber that joins late sees only subsequent actions. Design consequences:

- Subscribe to `Actions` (or effects) **before** dispatching the actions you care about.
- `store.Select(...)` behaves differently: it is backed by a `BehaviorSubject`, so a new subscriber immediately receives the **current** projected value, then updates. This is what makes it safe to bind UI at any point in the lifecycle.

## DistinctUntilChanged: The Library's Core Guarantee

Every reactive read in the library ends with `DistinctUntilChanged`:

```csharp
public IObservable<TResult> Select<TResult>(Func<StoreState, TResult> selector) =>
    _stateSubject.Select(selector).DistinctUntilChanged();
```

This is why you can dispatch freely: subscribers fire only when the *projected value* changes, not on every state change. Combined with selector memoization, the cost of an irrelevant dispatch is one reference comparison.

`DistinctUntilChanged` uses `EqualityComparer<T>.Default` — which is why immutable **records** are the recommended state shape: they give structural equality, so "changed" means "actually different data". For finer control, attach a custom comparer to the selector itself (see [Selectors](selectors.md)).

## The Operators You Actually Need

A small set covers almost all Redux usage:

**Filtering**

```csharp
actions.OfType<SavePersonAction>()          // by CLR type
actions.Action<ThunkFulfilled>("Load/fulfilled")  // by IAction name
actions.Where(a => a is not ThunkPending)   // arbitrary predicate
```

**Transforming**

```csharp
actions.OfType<SearchAction>().Select(a => new RunSearchAction(a.Text))
```

**Side effects without changing the stream**

```csharp
actions.Do(a => logger.LogInformation("{Action}", a))
```

**Async, with a concurrency choice**

```csharp
// Serialize: every item processed, in order (saves, writes)
.Select(a => Observable.FromAsync(() => api.Save(a)))
.Concat()

// Latest wins: cancel the in-flight one (search, navigation)
.Select(a => Observable.FromAsync(ct => api.Search(a.Text, ct)))
.Switch()

// Parallel: all at once (fan-out fetches)
.Select(a => Observable.FromAsync(() => api.Get(a.Id)))
.Merge()
```

**Flattening to multiple actions**

```csharp
.SelectMany(result => new object[] { new DoneAction(result), new RefreshAction() })
```

**Timing**

```csharp
actions.OfType<SearchTextChangedAction>().Throttle(TimeSpan.FromMilliseconds(300))
actions.OfType<StartAction>().Select(_ => Observable.Interval(TimeSpan.FromSeconds(30))).Switch()
```

**Error containment**

```csharp
.Catch<(object), Exception>(ex => Observable.Empty<object>())   // drop failures
.Catch<(object), Exception>(ex => Observable.Return(new FailedAction(ex)))  // convert to action
```

**Combining with state**

```csharp
actions.WithLatestFrom(store.Select(CartSelector), (a, cart) => (a, cart))
```

## Bridging Tasks and Observables

Two directions come up constantly:

**Task → Observable** (inside effects):

```csharp
.Select(a => Observable.FromAsync(ct => service.DoWorkAsync(a, ct)))
.Concat()
```

`FromAsync` also flows cancellation: when the subscription is disposed or `Switch` cancels, the `CancellationToken` fires.

**Observable → Task** (thunk results):

```csharp
var result = await store.DispatchThunk(MyThunks.Load, id).ToTask();
```

`ToTask()` subscribes, completes on the first `ThunkFulfilled`, and rethrows on `ThunkRejected`.

## Disposal

Every `Subscribe` returns an `IDisposable`. In long-lived objects (stores, services) subscriptions can live as long as the process. In components they **must** be disposed with the component, or you leak the component and keep receiving callbacks:

```csharp
public class UserViewModel : IDisposable
{
    private readonly IDisposable _subscription;

    public UserViewModel(ReduxStore store)
    {
        _subscription = store.Select(UserSelectors.CurrentUser)
            .Subscribe(user => User = user);
    }

    public User? User { get; private set; }

    public void Dispose() => _subscription.Dispose();
}
```

In Blazor, dispose in `Dispose`; in MAUI/WPF, dispose when the page/view model dies. Effects and thunk-internal subscriptions are managed by the library — you never dispose those yourself.

## Threading

- `Dispatch` is **blocking**: it queues the action(s) and returns only after the store's single processor thread has run the full pipeline. Reducers and middleware always run on that processor thread, never on yours. See [Threading](threading.md) for the full model.
- Because notifications fire on the processor thread, subscribers (including Blazor/MAUI render triggers) should marshal to the UI thread with Rx scheduling rather than blocking:

```csharp
store.Select(MySelectors.User)
    .ObserveOnDispatcher()   // or a captured SynchronizationContext scheduler
    .Subscribe(user => UpdateUi(user));
```

- State is immutable, so **reading** `store.State` or applying selectors from any thread is safe.

## See Also

- [Threading](threading.md) — the single-threaded core and thread-safe dispatch
- [Effects](effects.md) — pipelines in practice
- [Selectors](selectors.md) — memoization and deduplication
- [Thunks](thunks.md) — task/observable bridging for async work
