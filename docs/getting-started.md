# Getting Started

This guide explains the core concepts of MentorLake.Redux and walks through building a small application store from scratch.

- [The Redux Loop](#the-redux-loop)
- [State](#state)
- [Actions](#actions)
- [Reducers](#reducers)
- [Creating the Store](#creating-the-store)
- [Dispatching](#dispatching)
- [Reading State](#reading-state)
- [Dependency Injection](#dependency-injection)
- [Assembly Registration](#assembly-registration)
- [Recommended Project Layout](#recommended-project-layout)
- [When to Use Redux](#when-to-use-redux)

## The Redux Loop

Every change to application state flows through one pipeline:

```text
        dispatch(action)
              │
              ▼
     ┌─────────────────┐      pure functions       ┌──────────────┐
     │  Reducers        │  (state, action) ──────►  │  New State    │
     └─────────────────┘                            └──────┬───────┘
              ▲                                            │
              │                                            ▼
        ┌──────────┐                              ┌────────────────┐
        │  Thunks   │  async work, API calls       │  Subscribers    │
        └──────────┘                              │  (selectors)    │
              ▲                                   └────────────────┘
              │
        UI events, timers, messages
```

Three rules make this predictable:

1. **State is immutable.** Reducers never mutate; they return new state objects. Old snapshots remain valid, which makes state safe to read from any thread and easy to time-travel or diff.
2. **Changes happen only through actions.** An action is a plain object describing *what happened*. Nothing else can change state.
3. **Reducers are pure.** Given the same state and action, a reducer always produces the same result. This makes updates trivial to test.

Side effects (HTTP calls, logging, navigation) live in **effects** and **thunks**, which observe the action stream and dispatch new actions when async work completes.

## State

State is organized into *feature states* — small immutable records, one per feature of your application:

```csharp
public record PersonState(string FirstName, string LastName);
public record AddressState(string ZipCode);
public record CartState(ImmutableList<string> ItemIds, decimal Total);
```

Use `record` types so that `with` expressions give you cheap structural copies:

```csharp
var updated = person with { FirstName = "Bob" }; // new object, other fields copied
```

The store holds all feature states in a `StoreState` container, keyed by type. You rarely touch `StoreState` directly — selectors do it for you (see [Selectors](selectors.md)).

> **Note:** `StoreState` equality is *reference* equality. A dispatch that produces a new feature state produces a new `StoreState` instance; a dispatch no reducer handled leaves the same instance in place, so subscribers don't fire spuriously.

## Actions

Actions are plain objects that describe something that happened:

```csharp
public record UpdateFirstNameAction(string FirstName);
public record ZipCodeUpdatedAction(string ZipCode);
public record SavePersonAction(PersonState Person);
```

There is no interface to implement and no registration step. The action's .NET type is its identity by default. See [Actions](actions.md) for named actions and the observable action stream.

## Reducers

Reducers declare how each feature state responds to actions. `FeatureReducer.Build` starts from an initial state and `.On<TAction>` registers handlers:

```csharp
public class MyReducers : IReducerFactory
{
    public FeatureReducerCollection Create() =>
    [
        FeatureReducer.Build(new PersonState("Hello", "World"))
            .On<UpdateFirstNameAction>((state, action) => state with { FirstName = action.FirstName })
            .On<UpdateLastNameAction>((state, action) => state with { LastName = action.LastName }),

        FeatureReducer.Build(new AddressState("12345"))
            .On<ZipCodeUpdatedAction>((state, action) => state with { ZipCode = action.ZipCode })
    ];
}
```

Key properties:

- Each `FeatureReducer<T>` owns exactly one feature state slice.
- Handlers return the new slice; if no handler matches, the slice is unchanged.
- Handlers must be pure — no I/O, no captured mutable state. Put async work in [effects](effects.md) or [thunks](thunks.md).

## Creating the Store

```csharp
var store = new ReduxStore();
store.RegisterReducers(new MyReducers());
```

`RegisterReducers` initializes each feature state and installs its action handlers. It accepts any number of `IReducerFactory` instances, `FeatureReducerCollection`s, or raw `IFeatureReducer`s.

## Dispatching

```csharp
store.Dispatch(new UpdateFirstNameAction("Bob"));
```

Dispatch is **synchronous**: reducers run immediately and `store.State` reflects the update before `Dispatch` returns. Any async work triggered by effects or thunks continues in the background and is not awaited.

Every dispatched action is also published to `store.Actions`, an `IObservable<object>` that effects, thunks, and your own subscriptions can observe.

## Reading State

You can read the current state directly:

```csharp
var person = store.State.GetFeatureState<PersonState>();
```

But the idiomatic way is a reactive selector subscription:

```csharp
store.Select(MySelectors.FirstName)
    .Subscribe(firstName => Console.WriteLine(firstName));
```

`store.Select` emits the current value immediately and then again only when the selected value actually changes (change detection is done with memoized selectors and `DistinctUntilChanged`). See [Selectors](selectors.md) for the full story.

## Dependency Injection

The library has no DI requirements, but it composes naturally with `Microsoft.Extensions.DependencyInjection`. The common pattern is to register all of your `IReducerFactory` and `IEffectsFactory` implementations and hand them to the store:

```csharp
var serviceProvider = new ServiceCollection()
    .AddTransient<IReducerFactory, MyReducers>()
    .AddTransient<IEffectsFactory, MyEffects>()
    .AddTransient<PersonService>()
    .BuildServiceProvider();

var store = new ReduxStore();
store.RegisterReducers(serviceProvider.GetServices<IReducerFactory>().ToArray());
store.RegisterEffects(serviceProvider.GetServices<IEffectsFactory>().ToArray());
```

Effects factories are instantiated by the container, so they can take services through constructor injection:

```csharp
public class MyEffects(PersonService personService) : IEffectsFactory
{
    public IEnumerable<Effect> Create() =>
    [
        EffectsFactory.Create(actions => actions
            .OfType<SavePersonAction>()
            .Do(action => personService.SavePerson(action.Person)))
    ];
}
```

For injecting services into *thunks*, use a custom `ThunkApi` — see [Custom Thunk API](custom-thunk-api.md).

## Assembly Registration

For applications that prefer convention over explicit wiring, `RegisterAssembly` scans an assembly for concrete (parameterless-constructor) implementations of `IReducerFactory`, `IFeatureReducer`, and `IEffectsFactory` and registers all of them:

```csharp
var store = new ReduxStore();
store.RegisterAssembly(typeof(MyReducers).Assembly);
```

This is convenient in single-assembly apps and tests. If your factories need constructor injection, use the DI pattern above instead.

## Recommended Project Layout

A small-to-medium application typically organizes Redux code by *feature*, not by technical role:

```text
src/
  MyApp/
    Features/
      Person/
        PersonState.cs          # record
        PersonActions.cs        # records
        PersonReducers.cs       # IReducerFactory contribution
        PersonSelectors.cs      # static selectors
        PersonThunks.cs         # async workflows
        PersonEffects.cs        # IEffectsFactory contribution
      Address/
        ...
    StoreSetup.cs               # builds and configures ReduxStore
```

Grouping by feature keeps everything that can change together in one place, and makes it obvious which actions, selectors, and async flows belong to a domain concept.

## When to Use Redux

**Good fit:**

- Client applications with shared, long-lived state (forms, wizards, carts, dashboards, editors).
- Workflows where you need an audit trail of what happened and why state changed.
- Teams that benefit from one prescribed way to update state.
- UI frameworks where you want state-driven rendering without ad-hoc event wiring.

**Poor fit:**

- Stateless request/response services — there is no long-lived state to manage.
- Data that is purely transient and local to one component.
- Systems where eventual/replicated state is the primary concern (Redux is a single-writer model).

## Next Steps

- [Actions](actions.md) — how actions flow and how to name them
- [Reducers](reducers.md) — the full reducer API
- [Selectors](selectors.md) — efficient, composable state reads
- [Effects](effects.md) — reacting to actions with Rx
- [Thunks](thunks.md) — async workflows with lifecycle actions
