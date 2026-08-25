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

Thunks receive a `ThunkApi` (or a subclass) with the current store state and `Dispatch`. Named wrappers produce stable lifecycle action names for reducers and subscribers (`Name/pending`, `Name/fulfilled`, `Name/rejected`).

#### Defining

Wrap work in `ThunkAction<TApi>` / `ThunkFunc<TApi, …>`. The first type argument is the API type the thunk expects (`ThunkApi` for the default API):

```csharp
public static class MyThunks
{
	public static ThunkAction<ThunkApi> NoArgNoReturnThunk = new(
		"NoArgNoReturnThunk",
		async api => await Task.Delay(1000));

	public static ThunkAction<ThunkApi, int> ThunkWithArgs = new(
		"ThunkWithArgs",
		async (api, i) => await Task.Delay(i));

	public static ThunkFunc<ThunkApi, string> ThunkWithReturnValue = new(
		"ThunkWithReturnValue",
		api => Task.FromResult("Hello World"));

	public static ThunkFunc<ThunkApi, int, string> ThunkWithArgAndReturnValue = new(
		"ThunkWithArgAndReturnValue",
		(api, i) => Task.FromResult(i.ToString()));

	public static ThunkAction<ThunkApi> ThunkUsingSelector = new(
		"ThunkUsingSelector",
		api =>
		{
			var firstName = MySelectors.FirstName.Apply(api.State);
			Console.WriteLine(firstName);
			return Task.CompletedTask;
		});

	public static ThunkAction<ThunkApi> ThunkDispatchingActions = new(
		"ThunkDispatchingActions",
		api =>
		{
			api.Dispatch(new MyAction());
			return Task.CompletedTask;
		});

	public static ThunkFunc<ThunkApi, string> ThunkDispatchingThunk = new(
		"ThunkDispatchingThunk",
		async api =>
		{
			var result = await api.DispatchThunk(ThunkWithReturnValue).ToTask();
			return $"from nested: {result}";
		});
}
```

Names are optional. Unnamed thunks still emit lifecycle actions with empty prefixes (`/pending`, `/fulfilled`, `/rejected`):

```csharp
// Construct without a name
var delay = new ThunkAction<ThunkApi>(async api => await Task.Delay(100));
var load = new ThunkFunc<ThunkApi, int, string>((api, id) => Task.FromResult(id.ToString()));

// Or assign from a Func via implicit conversion
ThunkAction<ThunkApi> save = api =>
{
	api.Dispatch(new MyAction());
	return Task.CompletedTask;
};

ThunkFunc<ThunkApi, int, int> doubleIt = (api, value) => Task.FromResult(value * 2);
```

If you need dependency injection for the *thunk definitions themselves*, declare them as instance members on a non-static class. For shared services used *inside* thunks, prefer a custom `ThunkApi` (below).

#### Usage

`store.DispatchThunk(...)` is sugar for the default `ThunkApi` (`store.Using<ThunkApi>().Dispatch(...)`). Pass a named wrapper, a bound callable, a lambda, or a method group (with optional args):

```csharp
// Named thunk wrapper (name comes from the wrapper; Bind is optional for no-arg wrappers)
store.DispatchThunk(MyThunks.NoArgNoReturnThunk);

// Still supported: Bind() then dispatch the callable
store.DispatchThunk(MyThunks.NoArgNoReturnThunk.Bind());

// Named multi-arg wrapper — pass args after the wrapper (or Bind first)
store.DispatchThunk(MyThunks.ThunkWithArgAndReturnValue, 123);
store.DispatchThunk(MyThunks.ThunkWithArgAndReturnValue.Bind(123));

// Lambda (no args)
store.DispatchThunk(async api =>
{
	api.Dispatch(new MyAction());
	await Task.Delay(100);
});

// Lambda with args
store.DispatchThunk(async (api, userId) =>
{
	var user = await LoadUserAsync(userId);
	api.Dispatch(new UserLoadedAction(user));
}, 42);

// Method group with args
store.DispatchThunk(LoadUser, 42);

// Nested dispatch from inside another thunk
var parent = new ThunkAction<ThunkApi>("Parent", async api =>
{
	await api.DispatchThunk(async inner =>
	{
		inner.Dispatch(new MyAction());
		await Task.CompletedTask;
	}).ToTask();

	var value = await api.DispatchThunk(MyThunks.ThunkWithReturnValue).ToTask();
});
```

Observe lifecycle and custom actions, or await the result as a task:

```csharp
// Pending action
store.DispatchThunk(MyThunks.NoArgNoReturnThunk).Actions
	.Action<ThunkPending>("NoArgNoReturnThunk/pending")
	.Subscribe(action =>
	{
		Console.WriteLine("Thunk started.");
	});

// Fulfilled action
store.DispatchThunk(MyThunks.ThunkWithArgAndReturnValue, 123).Actions
	.Action<ThunkFulfilled<string>>("ThunkWithArgAndReturnValue/fulfilled")
	.Subscribe(action =>
	{
		Console.WriteLine($"Returned value: {action.Result}");
	});

// Custom action
store.DispatchThunk(MyThunks.ThunkWithArgAndReturnValue, 123).Actions
	.OfType<MyAction>()
	.Subscribe(action =>
	{
		Console.WriteLine($"My action received!");
	});

// Task
var result = await store.DispatchThunk(MyThunks.ThunkWithArgAndReturnValue, 123).ToTask();
Console.WriteLine($"Returned value: {result}");

// Direct lambda as a task
var greeting = await store.DispatchThunk(_ => Task.FromResult("Hello")).ToTask();
```

#### Custom ThunkApi

Subclass `ThunkApi` when thunks need extra dependencies or helpers. Register a factory with `UseThunkApi`, then dispatch through `Using<TApi>()`, which returns a `ThunkDispatcher<TApi>`.

```csharp
public class AppThunkApi : ThunkApi
{
	public AppThunkApi(ThunkApiContext context, IUserService users) : base(context)
	{
		Users = users;
	}

	public IUserService Users { get; }
}

// Required before Using<AppThunkApi>() — missing factories throw InvalidOperationException
store.UseThunkApi(ctx => new AppThunkApi(ctx, userService));
```

Define thunks as ordinary methods, lambdas, or typed wrappers against the custom API (optionally cache the dispatcher):

```csharp
public static class AppThunks
{
	// Method groups
	public static async Task LoadUser(AppThunkApi api)
	{
		var user = await api.Users.GetAsync(1);
		api.Dispatch(new UserLoadedAction(user));
	}

	public static Task<string> GetDisplayName(AppThunkApi api, int id) =>
		api.Users.GetDisplayNameAsync(id);

	// Named wrappers work the same way with the custom API type
	public static ThunkAction<AppThunkApi> Refresh = new(
		"Refresh",
		async api => await api.Users.RefreshAsync());
}

// One-off
await store.Using<AppThunkApi>().Dispatch(AppThunks.LoadUser).ToTask();
var name = await store.Using<AppThunkApi>().Dispatch(AppThunks.GetDisplayName, 42).ToTask();
await store.Using<AppThunkApi>().Dispatch(AppThunks.Refresh).ToTask();

// Cached dispatcher
var thunks = store.Using<AppThunkApi>();
await thunks.Dispatch(async api =>
{
	var user = await api.Users.GetAsync(1);
	api.Dispatch(new UserLoadedAction(user));
}).ToTask();
```

Lifecycle names for ad-hoc work use the optional name argument on `Dispatch` (wrappers already carry a name):

```csharp
await store.Using<AppThunkApi>().Dispatch("LoadUser", AppThunks.LoadUser).ToTask();

var name = await store.Using<AppThunkApi>().Dispatch("GetDisplayName", AppThunks.GetDisplayName, 42).ToTask();
```

Nested dispatches pick an API the same way — `ThunkApi` also has `Using<TApi>()`:

```csharp
await store.Using<AppThunkApi>().Dispatch(async api =>
{
	await api.Using<AppThunkApi>().Dispatch(async inner =>
	{
		await inner.Users.RefreshAsync();
	}).ToTask();
}).ToTask();
```

`store.DispatchThunk(...)` remains sugar for `store.Using<ThunkApi>().Dispatch(...)`.

#### Thunk Reducer

```csharp
public record MyThunkState(int CallCount);

public class MyThunkReducers : IReducerFactory
{
	public FeatureReducerCollection Create() =>
	[
		FeatureReducer.Build(new MyThunkState(0))
			.On<ThunkFulfilled>("NoArgNoReturnThunk/fulfilled", (state, action) => state with { CallCount = state.CallCount + 1 })
	];
}
```
