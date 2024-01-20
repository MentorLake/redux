## Getting Started

Create state types:
```csharp
public record PersonState(string FirstName, string LastName);
public record AddressState(string ZipCode);
```

Create action types:

```csharp
public record UpdateFirstNameAction(string FirstName);
public record UpdateLastNameAction(string LastName);
public record ZipCodeUpdatedAction(string ZipCode);
```

Create reducers:
```csharp
public static class MyReducers
{
	public static readonly FeatureReducerCollection Reducers =
	[
		FeatureReducer.Build(new PersonState("Hello", "World"))
			.On<UpdateFirstNameAction>((state, action) => state with { FirstName = action.FirstName })
			.On<UpdateLastNameAction>((state, action) => state with { LastName = action.LastName }),

		FeatureReducer.Build(new AddressState("12345"))
			.On<ZipCodeUpdatedAction>((state, action) => state with { ZipCode = action.ZipCode })
	];
}
```

Create and setup redux store:
```csharp
var store = new ReduxStore();
store.RegisterReducers(MyReducers.Reducers);
```

Dispatch actions:
```csharp
store.Dispatch(new UpdateFirstNameAction("Bob"));
```

## Selectors

Creating selectors:
```csharp
public class MySelectors
{
	public static readonly ISelector<PersonState> Person = SelectorFactory.CreateFeature<PersonState>();
	public static readonly ISelector<string> FirstName = SelectorFactory.Create(Person, s => s.FirstName);

	public static readonly ISelector<AddressState> Address = SelectorFactory.CreateFeature<AddressState>();
	public static readonly ISelector<string> ZipCode = SelectorFactory.Create(Address, s => s.ZipCode);
}
```

Using a selector:
```csharp
store.Select(MySelectors.FirstName).Subscribe(firstName => Console.WriteLine(firstName));
```

Custom comparison function:
```csharp
private bool CompareFirstNamesOnly(PersonState x, PersonState y)
{
	if (ReferenceEquals(x, y)) return true;
	if (x == null || y == null) return false;
	return x.FirstName.Equals(y.FirstName);
}
```

```csharp
var personSelector = SelectorFactory.Create(MySelectors.Person, p => p, CompareFirstNamesOnly);
// or
var personSelector = SelectorFactory.Create(MySelectors.Person.WithComparer(CompareFirstNamesOnly), p => p);
```

## Effects

```csharp
public class MyEffects(PersonService personService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		EffectsFactory.CreateEffect<SavePersonAction>(async action =>
		{
			await personService.SavePerson(action.Person);
			return new List<object>() { new SavePersonCompleteAction() };
		})
	};
}
```

It's best to register effects by resolving them from a DI container.  For example:
```csharp
var serviceCollection = new ServiceCollection();
serviceCollection.AddTransient<IEffectsFactory, MyEffects>();
serviceCollection.AddTransient<PersonService>();
var serviceProvider = serviceCollection.BuildServiceProvider();

var store = new ReduxStore();
store.RegisterEffects(serviceProvider.GetServices<IEffectsFactory>().ToArray());
```
