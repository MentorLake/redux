# Effects

Effects are Rx pipelines that react to dispatched actions and run side effects — HTTP calls, logging, storage, navigation — outside of reducers. They keep reducers pure while still letting your application respond to events.

- [When to Use Effects vs Thunks](#when-to-use-effects-vs-thunks)
- [Defining Effects](#defining-effects)
- [Dispatching from Effects](#dispatching-from-effects)
- [Registration](#registration)
- [Rx Patterns for Effects](#rx-patterns-for-effects)
- [Cancellation Strategies](#cancellation-strategies)
- [Error Handling and Retry](#error-handling-and-retry)
- [Best Practices](#best-practices)

## When to Use Effects vs Thunks

Both effects and thunks handle side effects, but they are triggered differently:

| | **Effects** | **Thunks** |
|---|---|---|
| Triggered by | Any action on the store's action stream | An explicit `DispatchThunk` call |
| Analogy | Event handler / listener | Async function with a result |
| Result | None (or follow-up actions) | A return value you can `await` |
| Lifecycle actions | Whatever you emit | Automatic `pending`/`fulfilled`/`rejected` |
| Typical use | "When X happens, also do Y" | "Perform this async operation now and tell me the outcome" |

Use an **effect** when something should happen *in response to* an action, with no caller waiting. Use a **thunk** when a component or another thunk *initiates* async work and wants the result.

## Defining Effects

An effect is a function from the action stream to another observable, created with `EffectsFactory.Create`:

```csharp
public class MyEffects(PersonService personService) : IEffectsFactory
{
    public IEnumerable<Effect> Create() => new[]
    {
        // Fire-and-forget: run something when an action occurs
        EffectsFactory.Create(actions => actions
            .OfType<SavePersonAction>()
            .Do(action => personService.SavePerson(action.Person))),
    };
}
```

By default (`EffectConfig { Dispatch = false }`), whatever the pipeline emits is discarded — the effect exists purely for its side effects (the `Do` operator above).

## Dispatching from Effects

Set `Dispatch = true` and every value your pipeline emits is dispatched into the store as a new action:

```csharp
EffectsFactory.Create(actions => actions
    .OfType<SavePersonAction>()
    .Select(action => Observable.FromAsync(() => personService.SavePersonAsync(action.Person)))
    .Concat()
    .SelectMany(_ => new object[] { new SavePersonCompleteAction(), new SomeOtherAction() }),
    new EffectConfig { Dispatch = true })
```

This pipeline:

1. Watches for `SavePersonAction`.
2. Maps each to an async save operation and **serializes** them with `Concat()` (saves never overlap).
3. On completion, emits two follow-up actions which the store dispatches.

`Dispatch = true` turns an effect into an action-to-action transformer — the standard way to convert async completions into state updates.

## Registration

Effects are registered on the store, usually from DI-created factories:

```csharp
var serviceProvider = new ServiceCollection()
    .AddTransient<IEffectsFactory, MyEffects>()
    .AddTransient<PersonService>()
    .BuildServiceProvider();

var store = new ReduxStore();
store.RegisterEffects(serviceProvider.GetServices<IEffectsFactory>().ToArray());

// Or directly:
store.RegisterEffects(EffectsFactory.Create(actions => actions.Do(Log)));
```

Effects start listening immediately and live for the lifetime of the store. Each effect subscribes to the action stream once at registration.

## Rx Patterns for Effects

Effects are plain `System.Reactive`, so the operator toolbox is your API. The patterns below cover most real-world needs.

**Call an API and dispatch the result:**

```csharp
EffectsFactory.Create(actions => actions
    .OfType<LoadUserAction>()
    .Select(a => Observable.FromAsync(ct => userService.GetAsync(a.UserId, ct)))
    .Switch() // cancel the previous load if a new one starts
    .Select(user => (object)new UserLoadedAction(user))
    .Catch<(object), Exception>(ex => Observable.Return(new UserLoadFailedAction(ex.Message))),
    new EffectConfig { Dispatch = true })
```

`Switch` is the right choice for "latest wins" (search-as-you-type); `Concat` is right for "all of them, in order" (saves, analytics).

**Debounce:**

```csharp
EffectsFactory.Create(actions => actions
    .OfType<SearchTextChangedAction>()
    .Throttle(TimeSpan.FromMilliseconds(300))
    .Select(a => new RunSearchAction(a.Text)),
    new EffectConfig { Dispatch = true })
```

**Fire multiple actions from one event:**

```csharp
.SelectMany(result => new object[]
{
    new LoginSucceededAction(result.User),
    new LoadPreferencesAction(result.User.Id)
})
```

**Combine with latest state** using `WithLatestFrom` over selectors:

```csharp
EffectsFactory.Create(actions => actions
    .OfType<SubmitOrderAction>()
    .WithLatestFrom(store.Select(CartSelectors.CartItems), (a, items) => (a, items))
    .Select(x => Observable.FromAsync(() => orderApi.SubmitAsync(x.a.Order, x.items)))
    .Concat()
    .Select(_ => new OrderSubmittedAction()),
    new EffectConfig { Dispatch = true })
```

**Timer / polling effects** (not triggered by an action — use a starting action to gate):

```csharp
EffectsFactory.Create(actions => actions
    .OfType<StartPollingAction>()
    .Select(_ => Observable.Interval(TimeSpan.FromSeconds(30)))
    .Switch()
    .Select(_ => new RefreshAction()),
    new EffectConfig { Dispatch = true })
```

## Cancellation Strategies

When a new trigger arrives while an effect's work is still running, the strategy decides what happens. Build effects fluently with `Effect.Build()`:

```csharp
Effect.Build()
    .On<SearchAction>()
    .Debounce(TimeSpan.FromMilliseconds(300))
    .TakeLatest()   // cancel the previous in-flight search
    .Handle(action => Observable.FromAsync(ct =>
        api.Search(((SearchAction) action).Term, ct)))
    .DispatchResults()
```

| Strategy | Behavior |
|----------|----------|
| `TakeEvery()` | run all occurrences concurrently (default) |
| `TakeLatest()` | dispose (cancel) in-flight work when a new trigger arrives |
| `Exhaust()` | ignore new triggers while work is in flight |

`Debounce` and `Throttle` shape the trigger stream before the strategy runs. Cancellation propagates through disposal: the token given to `Observable.FromAsync` inside `Handle` is cancelled, stopping token-aware work. See [Cancellation](cancellation.md) for details.

## Error Handling and Retry

Registered effects are wrapped in `Retry()`: if an effect's pipeline terminates with an error, it resubscribes to the action stream and continues listening for future actions. However, **the failed pipeline itself is abandoned** — in-flight work in that chain is lost.

Because `Retry` restarts the whole pipeline, prefer handling errors *inside* the pipeline so they never terminate it:

```csharp
EffectsFactory.Create(actions => actions
    .OfType<SaveThingAction>()
    .Select(a => Observable.FromAsync(() => api.SaveAsync(a.Thing))
        .Catch<Unit, Exception>(ex =>
        {
            logger.LogError(ex, "save failed");
            return Observable.Return(Unit.Default); // swallow per-item failure
        }))
    .Concat()
    .Select(_ => new SaveCompleteAction()),
    new EffectConfig { Dispatch = true })
```

For effects that must surface failures to the store, catch and dispatch a failure action instead of letting the exception escape.

## Best Practices

1. **Effects never touch state directly.** They observe actions and dispatch actions; only reducers change state.
2. **Choose `Switch` vs `Concat` deliberately.** Wrong choice causes lost updates or stale responses.
3. **Handle errors inside the pipeline.** Don't rely on `Retry` as your error strategy.
4. **One effect, one concern.** Small pipelines compose better and are easier to test.
5. **Constructor-inject services** via `IEffectsFactory` implementations registered in DI.

## See Also

- [Thunks](thunks.md) — caller-initiated async work
- [Cancellation](cancellation.md) — strategies, debounce/throttle, and token propagation
- [Actions](actions.md) — the stream effects observe
- [Reactive Programming](reactive-programming.md) — Rx concepts used here
