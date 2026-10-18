# MentorLake.Redux

[![NuGet](https://img.shields.io/nuget/v/MentorLake.Redux.svg)](https://www.nuget.org/packages/MentorLake.Redux)

A Redux implementation for .NET built on [System.Reactive](https://github.com/dotnet/reactive). It brings the Redux pattern — a single immutable store, pure reducers, and observable state — to C# with a first-class reactive programming model.

MentorLake.Redux is UI-framework agnostic. Use it in Blazor, MAUI, WPF, console applications, services, or anywhere you need predictable, auditable application state.

## Why Redux on .NET?

Application state gets hard when it is scattered across mutable objects, updated from arbitrary places, and read inconsistently while it changes. Redux constrains state management to a single, predictable loop:

```text
Action  →  Reducer  →  New State  →  Subscribers notified
```

- **Single source of truth.** All application state lives in one immutable `StoreState`.
- **Pure, testable updates.** Reducers are functions: `(state, action) => new state`. No I/O, no surprises.
- **Everything is a stream.** Both actions and state are `IObservable`, so you compose, filter, and react with the full Rx operator toolbox.
- **Structured async.** Thunks emit standardized `pending`/`fulfilled`/`rejected` lifecycle actions, so loading and error states are just more reducer logic.

## Features

| Feature | What it gives you |
|---------|-------------------|
| **Store** | Immutable state container with thread-safe dispatch serialized on a single processor thread |
| **Feature reducers** | Fluent, type-safe reducers per state slice with `FeatureReducer.Build(...).On<TAction>(...)` |
| **Memoized selectors** | Composable selectors (up to 7 inputs) that only recompute when inputs change, with custom comparers |
| **Middleware** | Intercept, rewrite, guard, or short-circuit every dispatch before it reaches reducers |
| **Batching** | `store.Dispatch(a1, a2, ...)` applies multiple actions as one atomic queue item with a single state notification |
| **Effects** | Rx pipelines that react to dispatched actions — API calls, logging, navigation — decoupled from reducers |
| **Thunks** | Async work units with named lifecycle actions, results, nesting, and dependency injection via custom `ThunkApi` |
| **Cancellation** | Cancellable thunks with cancellation groups (take-latest) and declarative effect strategies (`TakeLatest`, `TakeEvery`, `Exhaust`, debounce/throttle) |
| **ConflateLatest** | Thread-safe conflation operator for slow consumers — newest value wins, at most one delivery in flight, with `IScheduler` and `SynchronizationContext` overloads |
| **DataTable** | Immutable normalized entity collection (`ById` + `AllIds`) for list/entity state |
| **Assembly registration** | Auto-discover and register reducers and effects from an assembly |
| **DI friendly** | Works with `Microsoft.Extensions.DependencyInjection` out of the box |

## Installation

```bash
dotnet add package MentorLake.Redux
```

Requires .NET 10 or later. The only dependency is `System.Reactive`.

## Quick Start

```csharp
using MentorLake.Redux;
using MentorLake.Redux.Reducers;

// 1. State is an immutable record
public record PersonState(string FirstName, string LastName);

// 2. Actions are plain objects
public record UpdateFirstNameAction(string FirstName);

// 3. Reducers map actions to new state
public class MyReducers : IReducerFactory
{
    public FeatureReducerCollection Create() =>
    [
        FeatureReducer.Build(new PersonState("Hello", "World"))
            .On<UpdateFirstNameAction>((state, action) => state with { FirstName = action.FirstName })
    ];
}

// 4. Create the store and register reducers
var store = new ReduxStore();
store.RegisterReducers(new MyReducers());

// 5. Dispatch actions — safe from any thread. When Dispatch returns,
//    reducers have run and state notifications have fired.
store.Dispatch(new UpdateFirstNameAction("Bob"));

// 6. Observe state reactively
store.Select(s => s.GetFeatureState<PersonState>())
    .Subscribe(person => Console.WriteLine(person.FirstName)); // prints "Bob"
```

> **Threading in one paragraph:** the store serializes all dispatches through a
> single processor thread, so concurrent `Dispatch` calls from any thread are safe
> and nothing is lost. Subscribers run on that processor thread — if a handler is
> slow (rendering, I/O), hand off with `ConflateLatest(scheduler)` or `ObserveOn`
> instead of blocking it. See [Threading](docs/threading.md) and
> [Slow Consumers](docs/slow-consumers.md).

## Documentation

| Document | Description |
|----------|-------------|
| [Getting Started](docs/getting-started.md) | Core concepts, wiring up a store, DI integration, project layout |
| [Actions](docs/actions.md) | Action objects, the action stream, named actions with `IAction` |
| [Middleware](docs/middleware.md) | Intercepting, rewriting, and guarding dispatches before reducers |
| [Batching](docs/batching.md) | Coalescing multiple dispatches into one state notification |
| [Threading](docs/threading.md) | The single-threaded core, thread-safe dispatch, and the dispatch contract |
| [Best Practices](docs/best-practices.md) | Do's and don'ts: middleware, subscribers, thread hand-off, async work |
| [Slow Consumers](docs/slow-consumers.md) | Backpressure patterns: `Concat`, `Switch`, `Sample`, `ConflateLatest` |
| [Error Handling](docs/error-handling.md) | Dispatch failures, the `Errors` stream, and batch failure semantics |
| [Reducers](docs/reducers.md) | Feature reducers, action handlers, factories, and immutability patterns |
| [Selectors](docs/selectors.md) | Memoized, composable selectors and reactive state queries |
| [Effects](docs/effects.md) | Rx-based side-effect pipelines triggered by actions |
| [Thunks](docs/thunks.md) | Async work with lifecycle actions, results, and nesting |
| [Cancellation](docs/cancellation.md) | Cancellable thunks, cancellation groups, and effect strategies |
| [Custom Thunk API](docs/custom-thunk-api.md) | Injecting services into thunks with `ThunkApi` subclasses |
| [DataTable](docs/datatable.md) | Normalized, immutable entity collections |
| [Reactive Programming](docs/reactive-programming.md) | How Rx is used throughout the library |
| [Testing](docs/testing.md) | Unit testing reducers, selectors, thunks, and effects |
| [Blazor View Models](docs/blazor-viewmodels.md) | Component pattern: select, `TakeUntil`, subscribe, dispose |

## Building from Source

```bash
dotnet restore
dotnet build
dotnet test
```

The solution targets .NET 10. Versioning is managed with [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) (`version.json`); pushes to `main` are tested, packed, released to GitHub Releases, and published to NuGet by GitHub Actions.

## License

[MIT](LICENSE.md)
