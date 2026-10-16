# Actions

Actions are the only way state changes in MentorLake.Redux. An action is any object that describes something that happened in your application.

- [Plain Actions](#plain-actions)
- [The Action Stream](#the-action-stream)
- [Named Actions (IAction)](#named-actions-iaction)
- [Filtering Actions](#filtering-actions)
- [Conventions](#conventions)

## Plain Actions

There is no base class or interface required. A record is the recommended shape because it gives you value equality and immutability for free:

```csharp
public record UpdateFirstNameAction(string FirstName);
public record UpdateLastNameAction(string LastName);
public record ZipCodeUpdatedAction(string ZipCode);
public record SavePersonAction(PersonState Person);
```

Dispatching an action sends it through the store:

```csharp
store.Dispatch(new UpdateFirstNameAction("Bob"));
```

The store runs every registered reducer whose action type matches, then publishes the action to subscribers. Dispatch is synchronous and returns after reducers have finished; async work continues elsewhere (see [thunks](thunks.md)).

Actions can carry arbitrary payloads, including other domain objects — but keep payloads plain data. Passing mutable objects into state defeats immutability:

```csharp
// Good: immutable payload
public record SavePersonAction(PersonState Person);

// Avoid: mutable payload that callers can change after dispatch
public record SavePersonAction(PersonDto mutableDto);
```

## The Action Stream

Every action dispatched through the store is also published on `store.Actions`:

```csharp
IObservable<object> actions = store.Actions;
```

This is a hot, broadcast stream: every subscriber sees every action, in dispatch order. It is the input to [effects](effects.md), and it is what thunk result observables are built from.

You can subscribe directly for logging or debugging:

```csharp
store.Actions.Subscribe(action =>
    Console.WriteLine($"{DateTime.Now:O} {action.GetType().Name}"));
```

A useful pattern is a "state audit" effect that logs every action in development builds:

```csharp
EffectsFactory.Create(actions => actions
    .Do(action => logger.LogInformation("action: {Action}", action.GetType().Name)));
```

## Named Actions (IAction)

By default, a reducer matches an action by its .NET type (`typeof(TAction).FullName`). Implementing `IAction` lets an object carry an explicit **action name** instead, and reducers and subscribers can match on that name:

```csharp
public interface IAction
{
    string ActionName { get; }
}
```

This is primarily used for actions that share a type but represent different events — most importantly the thunk lifecycle actions (`ThunkPending`, `ThunkFulfilled`, `ThunkRejected`), which are distinguished only by name:

```csharp
public class ThunkFulfilled(string actionName) : IAction
{
    public string ActionName => $"{actionName}/fulfilled";
}
```

You can define your own named actions when you want string-keyed channels:

```csharp
public record NotificationAction(string ActionName, string Message) : IAction;

store.Dispatch(new NotificationAction("order/shipped", "Your order shipped"));
```

A reducer can then respond to a specific name:

```csharp
FeatureReducer.Build(new BannerState(null))
    .On<NotificationAction>("order/shipped",
        (state, action) => state with { Text = action.Message })
```

> **How matching works:** if an action implements `IAction`, reducers match against its `ActionName`; otherwise they match against the type's full name. A single action can therefore be handled by multiple reducers — every matching reducer runs, in registration order, each updating its own feature slice.

## Filtering Actions

`store.Actions` is `IObservable<object>`, so all standard Rx operators apply. The library adds two `Action` extension methods for name-based filtering:

```csharp
// Any IAction with a matching name
store.Actions
    .Action("NoArgNoReturnThunk/pending")
    .Subscribe(action => Console.WriteLine("Thunk started."));

// Strongly typed: IAction of a specific CLR type AND matching name
store.Actions
    .Action<ThunkFulfilled<string>>("LoadUser/fulfilled")
    .Subscribe(action => Console.WriteLine($"Result: {action.Result}"));
```

For type-based filtering (unnamed actions), use Rx's `OfType`:

```csharp
store.Actions
    .OfType<SavePersonAction>()
    .Subscribe(action => Console.WriteLine($"Saving {action.Person.FirstName}"));
```

Combinators work as expected for correlation-style logic — for example, pairing a request with its response:

```csharp
store.Actions
    .OfType<SavePersonAction>()
    .SelectMany(request =>
        store.Actions.OfType<SavePersonCompleteAction>().FirstAsync()
            .Select(_ => $"saved: {request.Person.FirstName}"))
    .Subscribe(Console.WriteLine);
```

## Conventions

- **Name actions in the past tense** (`UserLoadedAction`, not `LoadUserAction`) — they describe what *happened*, not a command to perform. Commands belong in thunks.
- **One action per event.** Don't overload a single action type with flags; make separate types.
- **Keep payloads immutable.** Records of records, immutable collections.
- **Use names (`IAction`) only when the type isn't enough** — the main case is thunk lifecycle actions and other dynamically named channels.

## See Also

- [Reducers](reducers.md) — responding to actions
- [Effects](effects.md) — reacting to actions asynchronously
- [Thunks](thunks.md) — emitting lifecycle actions
