using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;
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

public class MySelectors
{
	public static readonly ISelector<PersonState> Person = SelectorFactory.CreateFeature<PersonState>();
	public static readonly ISelector<string> FirstName = SelectorFactory.Create(Person, s => s.FirstName);

	public static readonly ISelector<AddressState> Address = SelectorFactory.CreateFeature<AddressState>();
	public static readonly ISelector<string> ZipCode = SelectorFactory.Create(Address, s => s.ZipCode);
}

public class PersonService
{
	public Task SavePersonAsync(PersonState state)
	{
		return Task.Delay(100);
	}

	public void SavePerson(PersonState state)
	{

	}
}

public class DispatchAsyncEffectsFactory(PersonService personService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		// Async effect with actions dispatched
		EffectsFactory.Create(actions => actions
			.OfType<SavePersonWithDispatchAction>()
			.Select(action => Observable.FromAsync(() => personService.SavePersonAsync(action.Person)))
			.Concat()
			.SelectMany(_ => new object[] { new SavePersonCompleteAction(), new SomeOtherAction() }),
			new EffectConfig() { Dispatch = true })
	};
}

public class NoDispatchEffectsFactory(PersonService personService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		// Vanilla effect with no dispatch
		EffectsFactory.Create(actions => actions
			.OfType<SavePersonWithoutDispatchAction>()
			.Do(action => personService.SavePerson(action.Person))),
	};
}

public class TestReducerFactory : IReducerFactory
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

[TestClass]
public class Demo
{
	private ReduxStore _store;
	private ServiceProvider _serviceProvider;

	[TestInitialize]
	public void Initialize()
	{
		_serviceProvider = new ServiceCollection()
			.AddTransient<IEffectsFactory, NoDispatchEffectsFactory>()
			.AddTransient<IEffectsFactory, DispatchAsyncEffectsFactory>()
			.AddTransient<IReducerFactory, TestReducerFactory>()
			.AddTransient<PersonService>()
			.BuildServiceProvider();

		_store = new ReduxStore();
		_store.RegisterReducers(_serviceProvider.GetServices<IReducerFactory>().ToArray());
		_store.RegisterEffects(_serviceProvider.GetServices<IEffectsFactory>().ToArray());
	}

	[TestMethod]
	[Timeout(1000)]
	public async Task ReducersAndSelectors()
	{
		var selectorTask = _store.Select(MySelectors.FirstName).Take(1).ToTask();

		await _store.Dispatch(new UpdateFirstNameAction("Bob"));
		await selectorTask;

		Assert.AreEqual("Bob", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	[Timeout(1000)]
	public async Task EffectsDispatch()
	{
		var dispatchedActionsTask = _store.Actions.Take(3).ToArray().ToTask();
		await _store.Dispatch(new SavePersonWithDispatchAction(new PersonState("Hello", "World")));

		var actions = await dispatchedActionsTask;
		Assert.AreEqual(typeof(SavePersonWithDispatchAction), actions[0].GetType());
		Assert.AreEqual(typeof(SavePersonCompleteAction), actions[1].GetType());
		Assert.AreEqual(typeof(SomeOtherAction), actions[2].GetType());
	}

	[TestMethod]
	public async Task EffectsNoDispatch()
	{
		var actionsCount = 0;
		_store.Actions.Subscribe(_ => actionsCount++);
		await _store.Dispatch(new SavePersonWithoutDispatchAction(new PersonState("Hello", "World")));
		Assert.AreEqual(1, actionsCount);
	}

	[TestMethod]
	public async Task SelectorComparison()
	{
		var changeCounter = 0;
		var personSelector = SelectorFactory.Create(MySelectors.Person, p => p, CompareFirstNamesOnly);
		_store.Select(personSelector).Take(1).Subscribe(_ => changeCounter++);

		await _store.Dispatch(new UpdateLastNameAction("Test"));
		await _store.Dispatch(new UpdateFirstNameAction("Bob"));

		Assert.AreEqual(1, changeCounter);
	}

	[TestMethod]
	public async Task InlineSelectorComparison()
	{
		var changeCounter = 0;

		var personSelector = SelectorFactory.Create(
			MySelectors.Person.WithComparer(CompareFirstNamesOnly),
			p => p);

		_store.Select(personSelector).Take(1).Subscribe(_ => changeCounter++);

		await _store.Dispatch(new UpdateLastNameAction("Test"));
		await _store.Dispatch(new UpdateFirstNameAction("Bob"));

		Assert.AreEqual(1, changeCounter);
	}

	private bool CompareFirstNamesOnly(PersonState x, PersonState y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null || y == null) return false;
		return x.FirstName.Equals(y.FirstName);
	}
}
