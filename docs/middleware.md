# Middleware

Middleware intercepts every action dispatched to the store **before** it reaches the reducers. This is the canonical Redux extension point (`store => next => action => ...`) and is the right place for cross-cutting concerns: logging, crash reporting, auth guards, action rewriting, and telemetry.

- [Defining Middleware](#defining-middleware)
- [Capabilities](#capabilities)
- [Ordering](#ordering)
- [Built-in Recipes](#built-in-recipes)

## Defining Middleware

Implement `IReduxMiddleware`:

```csharp
using MentorLake.Redux.Middleware;

public class LoggingMiddleware : IReduxMiddleware
{
    private ReduxStore _store = null;

    public void Initialize(ReduxStore store) => _store = store;

    public void Invoke(object action, NextMiddleware next)
    {
        Console.WriteLine($"dispatching {action}");
        next(action);
        Console.WriteLine($"state after: {_store.State}");
    }
}
```

Register it with `UseMiddleware`:

```csharp
var store = new ReduxStore();
store.UseMiddleware(new LoggingMiddleware());
```

For simple cases, use the delegate factory, which mirrors Redux's inline middleware functions:

```csharp
store.UseMiddleware(ReduxMiddleware.Create((action, next) =>
{
    if (action is AdminAction && !currentUser.IsAdmin) return; // guard
    next(action);
}));
```

## Capabilities

Inside `Invoke` you control what `next` sees:

| Goal | How |
|------|-----|
| Let the action through | `next(action)` |
| Short-circuit (swallow the action) | don't call `next` |
| Rewrite the action | `next(newAction)` |
| Replay/retry the action | call `next(action)` more than once |
| Delay the action | call `next(action)` later |
| Dispatch follow-up actions | `_store.Dispatch(...)` |

A short-circuited action never reaches the reducers and never appears on the store's `Actions` stream.

## Ordering

Middleware runs in registration order, wrapping the stages that follow:

```csharp
store.UseMiddleware(a); // outermost
store.UseMiddleware(b); // inner
// dispatch: a-before → b-before → reducers → b-after → a-after
```

Middleware can be added at any time; the pipeline is rebuilt on registration.

## Built-in Recipes

**Crash reporting** — observe reducer exceptions:

```csharp
store.UseMiddleware(ReduxMiddleware.Create((action, next) =>
{
    try { next(action); }
    catch (Exception e) { report.Submit(e, action); throw; }
}));
```

**Action allowlist for unauthenticated users:**

```csharp
store.UseMiddleware(ReduxMiddleware.Create((action, next) =>
{
    if (session.IsAuthenticated || action is not UserAction) next(action);
}));
```
