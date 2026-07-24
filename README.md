## Getting Started

#### Create state types
```csharp
public record PersonState(string FirstName, string LastName);
public record AddressState(string ZipCode);
```

#### Create action types
```csharp
public record UpdateFirstNameAction(string FirstName);
public record UpdateLastNameAction(string LastName);
public record ZipCodeUpdatedAction(string ZipCode);
```

#### Create reducers
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

#### Create and setup redux store
```csharp
var serviceProvider = new ServiceCollection()
    .AddTransient<IReducerFactory, MyReducers>()
    .BuildServiceProvider();

var store = new ReduxStore();
store.RegisterReducers(serviceProvider.GetServices<IReducerFactory>().ToArray());
```

#### Dispatch an action
```csharp
store.Dispatch(new UpdateFirstNameAction("Bob"));
```

Dispatch is synchronous: reducers run immediately and state is updated before the call returns. Asynchronous effects continue in the background and are not awaited.

## Selectors

Creating selectors
```csharp
public class MySelectors
{
	public static readonly ISelector<PersonState> Person = SelectorFactory.CreateFeature<PersonState>();
	public static readonly ISelector<string> FirstName = SelectorFactory.Create(Person, s => s.FirstName);

	public static readonly ISelector<AddressState> Address = SelectorFactory.CreateFeature<AddressState>();
	public static readonly ISelector<string> ZipCode = SelectorFactory.Create(Address, s => s.ZipCode);
}
```

#### Using a selector
```csharp
store.Select(MySelectors.FirstName).Subscribe(firstName => Console.WriteLine(firstName));
```

#### Custom comparison function
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
#### Defining
```csharp
public class MyEffects(PersonService personService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		// Vanilla effect with no dispatch
		EffectsFactory.Create(actions => actions
			.OfType<SavePersonAction>()
			.Do(action => personService.SavePerson(action.Person))),

		// Async effect with actions dispatched
		EffectsFactory.Create(actions => actions
			.OfType<SavePersonAction>()
			.Select(action => Observable.FromAsync(() => personService.SavePersonAsync(action.Person)))
			.Concat()
			.SelectMany(_ => new object[] { new SavePersonCompleteAction(), new SomeOtherAction() }),
			new EffectConfig() { Dispatch = true })
	};
}
```
#### Registration
```csharp
var serviceProvider = new ServiceCollection()
	.AddTransient<IEffectsFactory, MyEffects>()
	.AddTransient<PersonService>()
	.BuildServiceProvider();

var store = new ReduxStore();
store.RegisterEffects(serviceProvider.GetServices<IEffectsFactory>().ToArray());
```


## Thunks
#### Defining
```csharp
public static class MyThunks
{
	public static ThunkAction NoArgNoReturnThunk = new(
		"NoArgNoReturnThunk",
		async api => await Task.Delay(1000));

	public static ThunkAction<int> ThunkWithArgs = new(
		"ThunkWithArgs",
		async (api, i) => await Task.Delay(i));

	public static ThunkFunc<string> ThunkWithReturnValue = new(
		"ThunkWithReturnValue",
		(api) => Task.FromResult("Hello World"));

	public static ThunkFunc<int, string> ThunkWithArgAndReturnValue = new(
		"ThunkWithArgAndReturnValue",
		async (api, i) => Task.FromResult(i.ToString()));

	public static ThunkAction ThunkUsingSelector = new(
		"ThunkUsingSelector",
		api =>
		{
			var firstName = FirstNameSelector.Apply(api.State);
			Console.WriteLine(firstName);
			return Task.CompletedTask;
		});

	public static ThunkAction ThunkDispatchingActions = new(
		"ThunkDispatchingActions",
		api =>
		{
			api.Dispatch(new MyAction());
			return Task.CompletedTask;
		});

	public static ThunkFunc<string> ThunkDispatchingThunk = new(
		"ThunkDispatchingThunk",
		async api =>
		{
			var result = await api.DispatchThunk(ThunkWithReturnValue.Bind()).ToTask();
			return $"from nested: {result}";
		});
}
```
If you need dependency injection then declare your thunks as non-static in a non-static class.

#### Usage
```csharp
// Pending Action
store.DispatchThunk(MyThunks.NoArgNoReturnThunk.Bind()).Actions
	.Action<ThunkPending>("NoArgNoReturnThunk/pending")
	.Subscribe(action =>
	{
		Console.WriteLine("Thunk started.");
	});

// Fulfilled action
store.DispatchThunk(MyThunks.ThunkWithArgAndReturnValue.Bind(123)).Actions
	.Action<ThunkFulfilled<string>>("ThunkWithArgs/fulfilled")
	.Subscribe(action =>
	{
		Console.WriteLine($"Returned value: {action.Result}");
	});

// Custom action
store.DispatchThunk(MyThunks.ThunkWithArgAndReturnValue.Bind(123)).Actions
	.OfType<MyAction>()
	.Subscribe(action =>
	{
		Console.WriteLine($"My action received!");
	});

// Task
var result = await store.DispatchThunk(MyThunks.ThunkWithArgAndReturnValue.Bind(123)).ToTask();
Console.WriteLine($"Returned value: {result}");
```

#### Thunk Reducer

```csharp

public record MyThunkState(int CallCount);

public class MyThunkReducers : IReducerFactory
{
	public FeatureReducerCollection Create() =>
	[
		FeatureReducer.Build(new MyThunkState(0))
			.On<ThunkFinalized>("NoArgNoReturnThunk/fulfilled", (state, action) => state with { CallCount = state.CallCount + 1 })
	];
}
```
