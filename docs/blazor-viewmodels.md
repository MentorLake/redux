# Blazor View Models

Blazor components render snapshots. A Redux store is a stream. The view model pattern is the seam between them: the component owns no state of its own — it projects the store into a view model and re-renders whenever that projection changes.

- [The Pattern](#the-pattern)
- [Why Subscribe on Initialization](#why-subscribe-on-initialization)
- [Why `_viewModel` Is Nullable](#why-_viewmodel-is-nullable)
- [Why `TakeUntil(_disposeSubject)`](#why-takeuntildisposedispose)
- [Why `ConflateLatest`](#why-conflatelatest)
- [Why the Async Load Is a Thunk](#why-the-async-load-is-a-thunk)
- [Variations](#variations)
- [Pitfalls](#pitfalls)

## The Pattern

Two files. The component markup stays trivially declarative:

`Counter.razor`

```razor
@if (_viewModel is null)
{
    <p><em>Loading...</em></p>
}
else
{
    <p>Count: @_viewModel.Count</p>
    <p>Doubled: @_viewModel.Doubled</p>
    <button @onclick="Increment">+1</button>
}
```

The code lives in the code-behind file, not an `@code` block:

`Counter.razor.cs`

```csharp
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MentorLake.Redux;

public partial class Counter(ReduxStore store) : IDisposable
{
    private readonly Subject<Unit> _disposeSubject = new();
    private CounterViewModel? _viewModel;

    protected override async Task OnInitializedAsync()
    {
        // The state isn't meaningful until the load completes, so the
        // subscription starts after the load instead of before it.
        await store.Using<AppThunkApi>().Dispatch(CounterThunks.LoadCounter).ToTask();

        store.Select(CounterSelectors.CounterViewModel)
            .ConflateLatest(TaskPoolScheduler.Default)
            .TakeUntil(_disposeSubject)
            .Subscribe(vm =>
            {
                _viewModel = vm;
                InvokeAsync(StateHasChanged);
            });
    }

    private void Increment() => store.Dispatch(new IncrementAction());

    public void Dispose()
    {
        _disposeSubject.OnNext(Unit.Default);
        _disposeSubject.Dispose();
    }
}
```

The selector does all the shaping, so the component never computes anything:

```csharp
public static class CounterSelectors
{
    public static readonly ISelector<CounterViewModel> CounterViewModel =
        SelectorFactory.Create((StoreState s) =>
        {
            var counter = s.GetFeatureState<CounterState>();
            return new CounterViewModel(counter.Count, counter.Count * 2);
        });
}

public record CounterState(int Count);
public record CounterViewModel(int Count, int Doubled);
```

Memoization plus `DistinctUntilChanged` means the view model is rebuilt only when the underlying state actually changes, and every subscriber shares one evaluation.

The load is a **thunk**, not a component method. Services belong on a custom `ThunkApi`, not in the component's constructor:

```csharp
public class AppThunkApi(ThunkApiContext context, CounterService service) : ThunkApi(context)
{
    public CounterService Service { get; } = service;
}

public static class CounterThunks
{
    public static readonly ThunkAction<AppThunkApi> LoadCounter = new(
        "LoadCounter",
        async api =>
        {
            var count = await api.Service.GetCountAsync();
            api.Dispatch(new CounterLoadedAction(count));
        });
}
```

Registered once at startup:

```csharp
store.RegisterThunkApiFactory(ctx =>
    new AppThunkApi(ctx, serviceProvider.GetRequiredService<CounterService>()));
```

That's the whole pattern: **select → `ConflateLatest` → `TakeUntil` → subscribe → dispose**. Everything below explains why each piece is where it is.

## Why Subscribe on Initialization

The eager form subscribes in `OnInitialized`: the selector's current value enters the stream immediately, and the first view model lands a scheduler hop later. The component's first render may show the loading branch for a frame or two, then settles.

Subscribing in `OnAfterRenderAsync` instead would delay even the *start* of that pipeline by a full render round trip. There's no reason to pay for it twice.

**When to defer the subscription until after the load.** If the state the selector reads isn't meaningful until the load completes — a prefilled form, a report whose shape depends on the fetched data — then an eager subscription builds view models from placeholder state for no reason. In that case, override `OnInitializedAsync`, await the load thunk, and subscribe afterwards:

```csharp
protected override async Task OnInitializedAsync()
{
    await store.Using<AppThunkApi>().Dispatch(CounterThunks.LoadCounter).ToTask();

    store.Select(CounterSelectors.CounterViewModel)
        .ConflateLatest(TaskPoolScheduler.Default)
        .TakeUntil(_disposeSubject)
        .Subscribe(vm => { _viewModel = vm; InvokeAsync(StateHasChanged); });
}
```

Awaiting in `OnInitializedAsync` is what the lifecycle is designed for: Blazor holds the render until the returned task completes, then re-renders — so the component shows the loading branch while the fetch runs and data on the first real render. And because the load's `await` resumes on the renderer's synchronization context, the subscription is created back on the Blazor thread, not on a pool continuation.

The `TakeUntil(_disposeSubject)` guard is what makes this safe in a way it wouldn't be otherwise: if the user navigates away mid-load, `Dispose` fires the subject, and the subscription that starts afterwards terminates immediately. Without it, a component that no longer exists would subscribe to the store forever.

Two rules for deferred subscriptions:

- **Subscribe after the dispatch that completes the load.** `store.Select` emits the current value on subscription, so subscribing right after `CounterLoadedAction` still delivers the loaded state immediately.
- **Subscribe exactly once.** If the load can run more than once (retry, refresh), hoist the subscription out of the load path — subscribe once on first completion, or go back to eager subscription and filter the stream instead (`.Where(state => state.IsLoaded)`).

## Why `_viewModel` Is Nullable

With `ConflateLatest` in the chain, delivery is always asynchronous, so the very first render genuinely happens before any view model exists. The null branch isn't a defensive formality — it's the first frame of every component using this pattern, and it's also what renders while the load thunk is in flight.

It also covers the case where the selector has no value yet: if you compose the view with a `Skip`/`Where` filter, or the underlying feature state starts as `null` while data loads, the null branch is the honest rendering of "this view has no data yet", and it costs one `if`.

Do not paper over it with a default view model (`new CounterViewModel(0, 0)`). A fabricated zero hides the difference between "count is zero" and "we don't know yet", and the UI will confidently render the wrong thing.

## Why `TakeUntil(_disposeSubject)`

A subscription that outlives the component is a leak: the store keeps a reference to the component's lambda, the lambda keeps the component alive, and every state change re-renders a component that no longer exists (or throws trying).

The dispose-subject idiom makes teardown a single broadcast:

```csharp
private readonly Subject<Unit> _disposeSubject = new();
```

- `TakeUntil(_disposeSubject)` completes the selector stream when the subject fires, which disposes the subscription.
- `Dispose()` fires the subject once and disposes it.

The payoff shows up when a component has *several* subscriptions — a selector, an effect, a thunk-progress stream:

```csharp
protected override void OnInitialized()
{
    store.Select(CounterSelectors.CounterViewModel)
        .ConflateLatest(TaskPoolScheduler.Default)
        .TakeUntil(_disposeSubject)
        .Subscribe(vm => { _viewModel = vm; InvokeAsync(StateHasChanged); });

    store.Actions.OfType<SaveFailedAction>()
        .TakeUntil(_disposeSubject)
        .Subscribe(err => { _error = err.Message; InvokeAsync(StateHasChanged); });
}

public void Dispose()
{
    _disposeSubject.OnNext(Unit.Default);
    _disposeSubject.Dispose();
}
```

No `CompositeDisposable` bookkeeping, no fields per subscription — every `TakeUntil` chain dies with one line. It also composes with anything else you want to kill on dispose: `Throttle`, `Sample`, `Take(duration)`, per-item `TakeUntil(specificAction)`.

## Why the Async Load Is a Thunk

The component doesn't run the load — it dispatches one. `store.Using<AppThunkApi>().Dispatch(CounterThunks.LoadCounter)` hands the work to the store's async primitive, and the component only awaits its completion:

1. Dispatching the thunk automatically emits `LoadCounter/pending`, so loading state is reducer logic over lifecycle actions — no manual `LoadCounterAction`.
2. The service call happens inside the thunk, with the service arriving via the typed `AppThunkApi` — the component never knows `CounterService` exists.
3. The thunk dispatches `CounterLoadedAction`, which emits `LoadCounter/fulfilled` (or `rejected` with the exception if the service call throws).
4. `ToTask()` completes when the run fulfills or rejects, so `OnInitializedAsync` resumes exactly when the state is loaded — and the subscription delivers the first view model built from real data.

Notice what the component does **not** do: it never touches `_viewModel` and never calls `StateHasChanged` from the load path. There is exactly one writer to the view, and it's the subscription. That's what keeps the two async flows (load and subscription) from racing.

Failure handling comes for free: a throwing service call rejects the thunk, which dispatches `LoadCounter/rejected` (feed it to an error reducer or the `Errors` stream) and rethrows from `ToTask()`. If you don't want the rejection to surface in the component, catch around the `await` — the state has already recorded the failure.

The payoff of the thunk over an inline `LoadAsync` method: the load is named, observable (`Actions`), awaitable, cancellable, testable without a component, and reusable by any other initiator. See [Thunks](thunks.md) and [Custom Thunk API](custom-thunk-api.md).

## Why `ConflateLatest`

`store.Select` emits on the store's processor thread. Without a hand-off, everything downstream — the view-model projection, the subscribe handler, the render it triggers — runs inside the store's notification loop. A slow subscriber there delays every dispatch in the application, and a re-render triggered synchronously can re-enter the store.

`ConflateLatest(TaskPoolScheduler.Default)` (from `MentorLake.Redux`) does the hand-off and the protection in one operator:

- The processor thread's job ends at a lock — the handler runs on the pool.
- At most one delivery is in flight at a time, so the handler is *serial by construction* — no `Synchronize` needed. A handler that triggers work which feeds back into the stream can never interleave with itself.
- If the state changes while a delivery is in flight, only the **newest** value survives. A burst of dispatches collapses into the renders the component can actually use; stale view models are never painted.

```text
processor thread          pool
────────────────          ────
Select ──────────► ConflateLatest ──► Subscribe
(cheap, pure)        (hop + one-at-a-time + newest wins)  (_viewModel + StateHasChanged)
```

This is the right default for a renderer: it is *busy-driven*, not queue-driven. A component never needs to paint an intermediate state that was superseded before it started. If you genuinely need every value (rare in UI — e.g. animating each step), substitute `ObserveOn(TaskPoolScheduler.Default).Synchronize()`, and be aware that queues unboundedly under sustained overload (see [Slow Consumers](slow-consumers.md)).

## Variations

**Dispatch on the stream instead of an event handler.** If the view model exposes commands, keep the component free of action types entirely by having the view model carry delegates built by a thunk-dispatching service. The component still just calls `_viewModel.Increment()`.

**Multiple components, one selector.** Memoized selectors are shared: two components selecting `CounterSelectors.CounterViewModel` both subscribe, and the selector evaluates once per state change, not once per subscriber.

**Deliver on the dispatcher.** The example uses `InvokeAsync(StateHasChanged)` in the subscribe handler, which is thread-safe from any thread. The alternative is to marshal to the renderer's context at the end of the chain and drop the wrapper:

```csharp
store.Select(CounterSelectors.CounterViewModel)
    .ConflateLatest(TaskPoolScheduler.Default)
    .ObserveOnDispatcher()
    .TakeUntil(_disposeSubject)
    .Subscribe(vm => { _viewModel = vm; StateHasChanged(); });
```

The second `ObserveOn` is a delivery concern, not a protection concern — the processor thread is already protected by the conflation hop. Pick `InvokeAsync` or `ObserveOnDispatcher`, not both.

## Pitfalls

- **Forgetting to implement `IDisposable` at all.** The framework calls `Dispose` when the component is removed from the UI *only if the component type implements the interface*. With a code-behind, adding `: IDisposable` to the partial class is sufficient — the generated partial already supplies `ComponentBase`, and no `@implements IDisposable` directive is needed in the markup (that directive exists for components without code-behind files).
- **Re-subscribing on every parameter change.** Subscribe once in `OnInitialized`. If the view depends on a `[Parameter]`, select it into the view model (`store.Select(s => ...param-based projection...)` via a selector you rebuild in `OnParametersSet` *only when the parameter actually changes*) — do not tear down and re-subscribe per render.
- **Mutating `_viewModel` from the subscription without `StateHasChanged`.** The assignment alone doesn't trigger a render; the framework only re-renders when *it* causes an event or you tell it to.
- **Swallowing exceptions in the subscription.** A fault in the selector or handler terminates the stream silently. Route store failures through the `Errors` stream as described in [Error Handling](error-handling.md).

## See Also

- [Selectors](selectors.md) — memoized selectors and `DistinctUntilChanged`
- [Slow Consumers](slow-consumers.md) — strategies for bursty state
- [Threading](threading.md) — why `InvokeAsync`/`ObserveOnDispatcher` is needed
- [Best Practices](best-practices.md) — subscriber etiquette
- [Thunks](thunks.md) — where the actions the component dispatches come from
