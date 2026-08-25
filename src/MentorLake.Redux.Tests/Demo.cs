using System.Reactive.Linq;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;
using MentorLake.Redux.Thunks;
using Microsoft.Extensions.DependencyInjection;

namespace MentorLake.Redux.Tests;

public record PersonState(string FirstName, string LastName);
public record AddressState(string ZipCode);

public record UpdateFirstNameAction(string FirstName);
public record UpdateLastNameAction(string LastName);
public record ZipCodeUpdatedAction(string ZipCode);
public record SavePersonWithDispatchAction(PersonState Person);
public record SavePersonCompleteAction();
public record SomeOtherAction();
public record SavePersonWithoutDispatchAction(PersonState Person);

public class TestException : Exception { }

public static class TestThunks
{
	public static ThunkAction<ThunkApi> Test1 = new("Test1", _ => Task.CompletedTask);
	public static ThunkAction<ThunkApi> Test2 = new("Test2", _ => throw new TestException());
	public static ThunkFunc<ThunkApi, string> Test3 = new("Test3", _ => Task.FromResult("Hello"));
	public static ThunkFunc<ThunkApi, string, string> Test4 = new("Test4", (_, arg) => Task.FromResult(arg));
	public static ThunkAction<ThunkApi, Exception> Test5 = new("Test5", (_, arg) => throw arg);
	public static ThunkAction<ThunkApi> Test6 = new("Test6", _ => Task.CompletedTask);
	public static ThunkFunc<ThunkApi, string, string> TestLiveState = new("TestLiveState", (api, newName) =>
	{
		var before = MySelectors.FirstName.Apply(api.State);
		api.Dispatch(new UpdateFirstNameAction(newName));
		var after = MySelectors.FirstName.Apply(api.State);
		return Task.FromResult($"{before}->{after}");
	});
	public static ThunkFunc<ThunkApi, string, string> TestNestedThunk = new("TestNestedThunk", async (api, name) =>
	{
		var nestedResult = await api.DispatchThunk(Test4.Bind(name)).ToTask();
		return $"nested:{nestedResult}";
	});
	public static ThunkAction<ThunkApi> TestNestedThunkAction = new("TestNestedThunkAction", async api =>
	{
		await api.DispatchThunk(Test6.Bind()).ToTask();
	});
}

public class MySelectors
{
	public static readonly ISelector<PersonState> Person = SelectorFactory.CreateFeature<PersonState>();
	public static readonly ISelector<string> FirstName = SelectorFactory.Create(Person, s => s.FirstName);

	public static readonly ISelector<AddressState> Address = SelectorFactory.CreateFeature<AddressState>();
	public static readonly ISelector<string> ZipCode = SelectorFactory.Create(Address, s => s.ZipCode);
}

public class PersonService
{
	public void SavePerson(PersonState state)
	{
	}
}

public class ThunkState
{
	public int CallCount { get; set; }
}

public class DispatchEffectsFactory(PersonService personService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() =>
	[
		EffectsFactory.Create(actions => actions
				.OfType<SavePersonWithDispatchAction>()
				.Do(action => personService.SavePerson(action.Person))
				.SelectMany(_ => new object[] { new SavePersonCompleteAction(), new SomeOtherAction() }),
			new EffectConfig { Dispatch = true })
	];
}

public class NoDispatchEffectsFactory(PersonService personService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() =>
	[
		EffectsFactory.Create(actions => actions
			.OfType<SavePersonWithoutDispatchAction>()
			.Do(action => personService.SavePerson(action.Person)))
	];
}

public class TestReducerFactory : IReducerFactory
{
	public FeatureReducerCollection Create() =>
	[
		FeatureReducer.Build(new PersonState("Hello", "World"))
			.On<UpdateFirstNameAction>((state, action) => state with { FirstName = action.FirstName })
			.On<UpdateLastNameAction>((state, action) => state with { LastName = action.LastName }),

		FeatureReducer.Build(new AddressState("12345"))
			.On<ZipCodeUpdatedAction>((state, action) => state with { ZipCode = action.ZipCode }),

		FeatureReducer.Build(new ThunkState())
			.On<ThunkFulfilled>("Test6/fulfilled", (s, a) => new() { CallCount = s.CallCount + 1 })
	];
}

[TestClass]
public class Demo
{
	private ReduxStore _store;

	[TestInitialize]
	public void Initialize()
	{
		_store = new ReduxStore();

		var services = new ServiceCollection()
			.AddTransient<IEffectsFactory, NoDispatchEffectsFactory>()
			.AddTransient<IEffectsFactory, DispatchEffectsFactory>()
			.AddTransient<IReducerFactory, TestReducerFactory>()
			.AddTransient<PersonService>()
			.BuildServiceProvider();

		_store.RegisterReducers(services.GetServices<IReducerFactory>().ToArray());
		_store.RegisterEffects(services.GetServices<IEffectsFactory>().ToArray());
	}

	[TestMethod]
	public void ReducersAndSelectors()
	{
		_store.Dispatch(new UpdateFirstNameAction("Bob"));
		Assert.AreEqual("Bob", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public void EffectsDispatch()
	{
		var actions = new List<object>();
		using var _ = _store.Actions.Subscribe(actions.Add);

		_store.Dispatch(new SavePersonWithDispatchAction(new PersonState("Hello", "World")));

		Assert.AreEqual(3, actions.Count);
		Assert.AreEqual(1, actions.OfType<SavePersonWithDispatchAction>().Count());
		Assert.AreEqual(1, actions.OfType<SavePersonCompleteAction>().Count());
		Assert.AreEqual(1, actions.OfType<SomeOtherAction>().Count());
	}

	[TestMethod]
	public void EffectsNoDispatch()
	{
		var actionsCount = 0;
		using var _ = _store.Actions.Subscribe(_ => actionsCount++);

		_store.Dispatch(new SavePersonWithoutDispatchAction(new PersonState("Hello", "World")));
		Assert.AreEqual(1, actionsCount);
	}

	[TestMethod]
	public void SelectorComparison()
	{
		var emissions = new List<PersonState>();
		var personSelector = SelectorFactory.Create(MySelectors.Person, p => p, CompareFirstNamesOnly);
		using var _ = _store.Select(personSelector).Subscribe(emissions.Add);

		_store.Dispatch(new UpdateLastNameAction("Test"));
		_store.Dispatch(new UpdateFirstNameAction("Bob"));

		Assert.AreEqual(2, emissions.Count);
		Assert.AreEqual("Hello", emissions[0].FirstName);
		Assert.AreEqual("Bob", emissions[1].FirstName);
	}

	[TestMethod]
	public void InlineSelectorComparison()
	{
		var emissions = new List<PersonState>();
		var personSelector = SelectorFactory.Create(
			MySelectors.Person.WithComparer(CompareFirstNamesOnly),
			p => p);
		using var _ = _store.Select(personSelector).Subscribe(emissions.Add);

		_store.Dispatch(new UpdateLastNameAction("Test"));
		_store.Dispatch(new UpdateFirstNameAction("Bob"));

		Assert.AreEqual(2, emissions.Count);
	}

	[TestMethod]
	public void ThunkBasic()
	{
		var actions = new List<object>();
		using var _ = _store.DispatchThunk(TestThunks.Test1.Bind()).Actions.Subscribe(actions.Add);

		Assert.IsInstanceOfType(actions[0], typeof(ThunkPending));
		Assert.IsInstanceOfType(actions[1], typeof(ThunkFulfilled));
	}

	[TestMethod]
	public void ThunkException()
	{
		var testException = new Exception("ASDF");
		var actions = new List<object>();
		using var _ = _store.DispatchThunk(TestThunks.Test5.Bind(testException)).Actions.Subscribe(actions.Add);

		Assert.IsInstanceOfType(actions[0], typeof(ThunkPending));
		Assert.IsInstanceOfType(actions[1], typeof(ThunkRejected));
		Assert.AreSame(testException, ((ThunkRejected)actions[1]).Exception);
	}

	[TestMethod]
	public async Task ThunkReturnValue()
	{
		Assert.AreEqual("Hello", await _store.DispatchThunk(TestThunks.Test3.Bind()).ToTask());
		Assert.AreEqual("Hello", await _store.DispatchThunk(TestThunks.Test4.Bind("Hello")).ToTask());
	}

	[TestMethod]
	public async Task ThunkReducer()
	{
		await _store.DispatchThunk(TestThunks.Test6.Bind()).ToTask();
		Assert.AreEqual(1, _store.State.GetFeatureState<ThunkState>().CallCount);
	}

	[TestMethod]
	public async Task ThunkApiStateIsLive()
	{
		var result = await _store.DispatchThunk(TestThunks.TestLiveState.Bind("Bob")).ToTask();
		Assert.AreEqual("Hello->Bob", result);
		Assert.AreEqual("Bob", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public async Task ThunkApiDispatchThunkWithResult()
	{
		var result = await _store.DispatchThunk(TestThunks.TestNestedThunk.Bind("World")).ToTask();
		Assert.AreEqual("nested:World", result);
	}

	[TestMethod]
	public async Task ThunkApiDispatchThunkAction()
	{
		await _store.DispatchThunk(TestThunks.TestNestedThunkAction.Bind()).ToTask();
		Assert.AreEqual(1, _store.State.GetFeatureState<ThunkState>().CallCount);
	}

	[TestMethod]
	[ExpectedException(typeof(TestException))]
	public async Task ThunkExceptionTask()
	{
		await _store.DispatchThunk(TestThunks.Test2.Bind()).ToTask();
	}

	[TestMethod]
	public void EffectsValueTuple()
	{
		_store.RegisterEffects(EffectsFactory.Create(actions => actions.Select(_ => (1, 2))));
		_store.Dispatch(new UpdateLastNameAction("Test"));
		_store.Dispatch(new UpdateFirstNameAction("Bob"));
	}

	private static bool CompareFirstNamesOnly(PersonState x, PersonState y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null || y == null) return false;
		return x.FirstName.Equals(y.FirstName);
	}
}
