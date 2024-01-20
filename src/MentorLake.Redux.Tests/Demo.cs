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
public record SavePersonAction(PersonState Person);
public record SavePersonCompleteAction();

public class MySelectors
{
	public static readonly ISelector<PersonState> Person = SelectorFactory.CreateFeature<PersonState>();
	public static readonly ISelector<string> FirstName = SelectorFactory.Create(Person, s => s.FirstName);

	public static readonly ISelector<AddressState> Address = SelectorFactory.CreateFeature<AddressState>();
	public static readonly ISelector<string> ZipCode = SelectorFactory.Create(Address, s => s.ZipCode);
}

public class PersonService
{
	public Task SavePerson(PersonState state)
	{
		return Task.Delay(100);
	}
}
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

[TestClass]
public class Demo
{
	[TestMethod]
	[Timeout(1000)]
	public async Task ReducersAndSelectors()
	{
		var store = new ReduxStore();
		store.RegisterReducers(MyReducers.Reducers);
		var selectorTask =  store.Select(MySelectors.FirstName).Take(1).ToTask();

		await store.Dispatch(new UpdateFirstNameAction("Bob"));
		await selectorTask;

		Assert.AreEqual("Bob", store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	[Timeout(1000)]
	public async Task Effects()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddTransient<IEffectsFactory, MyEffects>();
		serviceCollection.AddTransient<PersonService>();
		var serviceProvider = serviceCollection.BuildServiceProvider();

		var store = new ReduxStore();
		store.RegisterReducers(MyReducers.Reducers);
		store.RegisterEffects(serviceProvider.GetServices<IEffectsFactory>().ToArray());

		var saveCompleteTask = store.ObserveAction<SavePersonCompleteAction>().Take(1).ToTask();
		await store.Dispatch(new SavePersonAction(new PersonState("Hello", "World")));
		await saveCompleteTask;
	}

	[TestMethod]
	[Timeout(1000)]
	public async Task SelectorComparison()
	{
		var changeCounter = 0;
		var personSelector = SelectorFactory.Create(MySelectors.Person, p => p, CompareFirstNamesOnly);
		var store = new ReduxStore();
		store.RegisterReducers(MyReducers.Reducers);
		store.Select(personSelector).Take(1).Subscribe(_ => changeCounter++);

		await store.Dispatch(new UpdateLastNameAction("Test"));
		await store.Dispatch(new UpdateFirstNameAction("Bob"));

		Assert.AreEqual(1, changeCounter);
	}

	[TestMethod]
	[Timeout(1000)]
	public async Task InlineSelectorComparison()
	{
		var changeCounter = 0;

		var personSelector = SelectorFactory.Create(
			MySelectors.Person.WithComparer(CompareFirstNamesOnly),
			p => p);

		var store = new ReduxStore();
		store.RegisterReducers(MyReducers.Reducers);
		store.Select(personSelector).Take(1).Subscribe(_ => changeCounter++);

		await store.Dispatch(new UpdateLastNameAction("Test"));
		await store.Dispatch(new UpdateFirstNameAction("Bob"));

		Assert.AreEqual(1, changeCounter);
	}

	private bool CompareFirstNamesOnly(PersonState x, PersonState y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null || y == null) return false;
		return x.FirstName.Equals(y.FirstName);
	}
}
