## Getting Started

Create your state types:
```
public record PersonState(string FirstName, string LastName);
public record AddressState(string ZipCode);
```

Create action types:

```
public record UpdateFirstNameAction(string FirstName);
public record UpdateLastNameAction(string LastName);
public record ZipCodeUpdatedAction(string ZipCode);
```

Create the store and register your reducers:

```
var store = new ReduxStore();

store.RegisterReducers(new FeatureReducerCollection()
{
	FeatureReducer.Build(new PersonState("Hello", "World"))
		.On<UpdateFirstNameAction>((state, action) => state with { FirstName = action.FirstName })
		.On<UpdateLastNameAction>((state, action) => state with { LastName = action.LastName }),
	FeatureReducer.Build(new AddressState(12345))
		.On<ZipCodeUpdatedAction>((state, action) => state with { ZipCode = action.ZipCode })
});
```
Dispatch actions:

```
store.Dispatch(new UpdateFirstNameAction("Bob"));
```

## Selectors

Creating selectors:
```
public class MySelectors
{
	public static readonly ISelector<PersonState> Person = SelectorFactory.CreateFeatureSelector<PersonState>();
	public static readonly ISelector<string> FirstName = SelectorFactory.CreateSelector(Person, s => s.FirstName);

	public static readonly ISelector<AddressState> Address = SelectorFactory.CreateFeatureSelector<AddressState>();
	public static readonly ISelector<string> ZipCode = SelectorFactory.CreateSelector(Address, s => s.ZipCode);
}
```

Using the selector:
```
store.Select(MySelectors.FirstName).Subscribe(firstName => Console.WriteLine(firstName));
```

## Effects

```
public class MyEffects(PersonService personService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		EffectsFactory.CreateEffect<SaveUserAction>(action => personService.SaveUser(action.User));
	};
}
```
It's best to register effects by resolving them from a DI container.  For example:
```
store.RegisterEffects(host.Services.GetServices<IEffectsFactory>().ToArray());
```
