using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;

namespace MentorLake.Redux.Tests;

public record CounterState(int Value);
public record NamedBumpAction(int Amount) : IAction
{
	public string ActionName => "counter/bump";
}
public record UnknownAction();

[TestClass]
public class ReduxStoreTests
{
	[TestMethod]
	public void Dispatch_Null_IsNoOp()
	{
		var store = CreatePersonStore();
		var before = store.State;
		store.Dispatch(null);
		Assert.AreSame(before, store.State);
	}

	[TestMethod]
	public void Dispatch_UnknownAction_DoesNotChangeFeatureState()
	{
		var store = CreatePersonStore();
		var before = store.State.GetFeatureState<PersonState>();
		store.Dispatch(new UnknownAction());
		Assert.AreEqual(before, store.State.GetFeatureState<PersonState>());
	}

	[TestMethod]
	public void RegisterReducers_AcceptsFeatureReducersDirectly()
	{
		var store = new ReduxStore();
		store.RegisterReducers(
			FeatureReducer.Build(new PersonState("A", "B"))
				.On<UpdateFirstNameAction>((s, a) => s with { FirstName = a.FirstName }));

		store.Dispatch(new UpdateFirstNameAction("Zed"));
		Assert.AreEqual("Zed", store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public void RegisterReducers_AcceptsFeatureReducerCollection()
	{
		var collection = new FeatureReducerCollection
		{
			FeatureReducer.Build(new PersonState("A", "B"))
				.On<UpdateFirstNameAction>((s, a) => s with { FirstName = a.FirstName }),
			FeatureReducer.Build(new AddressState("00000"))
				.On<ZipCodeUpdatedAction>((s, a) => s with { ZipCode = a.ZipCode })
		};

		var store = new ReduxStore();
		store.RegisterReducers(collection);

		store.Dispatch(new UpdateFirstNameAction("Cole"));
		store.Dispatch(new ZipCodeUpdatedAction("11111"));

		Assert.AreEqual("Cole", store.State.GetFeatureState<PersonState>().FirstName);
		Assert.AreEqual("11111", store.State.GetFeatureState<AddressState>().ZipCode);
	}

	[TestMethod]
	public void RegisterReducers_NamedIAction()
	{
		var store = new ReduxStore();
		store.RegisterReducers(
			FeatureReducer.Build(new CounterState(0))
				.On<NamedBumpAction>("counter/bump", (s, a) => s with { Value = s.Value + a.Amount }));

		store.Dispatch(new NamedBumpAction(3));
		store.Dispatch(new NamedBumpAction(2));

		Assert.AreEqual(5, store.State.GetFeatureState<CounterState>().Value);
	}

	[TestMethod]
	public async Task Actions_EmitsDispatchedActions()
	{
		var store = CreatePersonStore();
		var actionsTask = store.Actions.Take(2).ToArray().ToTask();

		store.Dispatch(new UpdateFirstNameAction("A"));
		store.Dispatch(new UpdateLastNameAction("B"));

		var actions = await actionsTask;
		Assert.IsInstanceOfType(actions[0], typeof(UpdateFirstNameAction));
		Assert.IsInstanceOfType(actions[1], typeof(UpdateLastNameAction));
	}

	[TestMethod]
	public void MultipleReducers_CanHandleSameAction()
	{
		var store = new ReduxStore();
		store.RegisterReducers(
			FeatureReducer.Build(new PersonState("Hello", "World"))
				.On<UpdateFirstNameAction>((s, a) => s with { FirstName = a.FirstName }),
			FeatureReducer.Build(new CounterState(0))
				.On<UpdateFirstNameAction>((s, _) => s with { Value = s.Value + 1 }));

		store.Dispatch(new UpdateFirstNameAction("X"));
		store.Dispatch(new UpdateFirstNameAction("Y"));

		Assert.AreEqual("Y", store.State.GetFeatureState<PersonState>().FirstName);
		Assert.AreEqual(2, store.State.GetFeatureState<CounterState>().Value);
	}

	[TestMethod]
	public void RegisterReducers_ViaIReducerFactory()
	{
		var store = new ReduxStore();
		store.RegisterReducers(new TestReducerFactory());

		store.Dispatch(new UpdateFirstNameAction("Z"));
		Assert.AreEqual("Z", store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public void RegisterEffects_AcceptsEffectArray()
	{
		var store = CreatePersonStore();
		var sideEffectCount = 0;

		store.RegisterEffects(
			EffectsFactory.Create(actions => actions
				.OfType<UpdateFirstNameAction>()
				.Do(_ => sideEffectCount++)));

		store.Dispatch(new UpdateFirstNameAction("Bob"));
		Assert.AreEqual(1, sideEffectCount);
	}

	[TestMethod]
	public void RegisterEffects_ViaIEffectsFactory()
	{
		var store = CreatePersonStore();
		var sideEffectCount = 0;

		store.RegisterEffects(new FirstNameSideEffectFactory(() => sideEffectCount++));
		store.Dispatch(new UpdateFirstNameAction("Bob"));

		Assert.AreEqual(1, sideEffectCount);
	}

	[TestMethod]
	public async Task RegisterEffects_DispatchesReturnedActions()
	{
		var store = CreatePersonStore();
		store.RegisterEffects(
			EffectsFactory.Create(
				actions => actions
					.OfType<UpdateFirstNameAction>()
					.Select(_ => new UpdateLastNameAction("FromEffect")),
				new EffectConfig { Dispatch = true }));

		var valuesTask = store.Select(s => s.GetFeatureState<PersonState>().LastName)
			.Take(2)
			.ToArray()
			.ToTask();

		store.Dispatch(new UpdateFirstNameAction("Bob"));
		var values = await valuesTask;

		CollectionAssert.AreEqual(new[] { "World", "FromEffect" }, values);
	}

	[TestMethod]
	public void RegisterEffects_IgnoresNullRunOrConfig()
	{
		var store = CreatePersonStore();
		store.RegisterEffects(
			new Effect { Run = null, Config = new EffectConfig() },
			new Effect { Run = actions => actions, Config = null });

		store.Dispatch(new UpdateFirstNameAction("Bob"));
		Assert.AreEqual("Bob", store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public void Dispatch_IsSynchronous_StateUpdatedBeforeReturn()
	{
		var store = CreatePersonStore();
		store.Dispatch(new UpdateFirstNameAction("Sync"));
		Assert.AreEqual("Sync", store.State.GetFeatureState<PersonState>().FirstName);
	}

	private static ReduxStore CreatePersonStore()
	{
		var store = new ReduxStore();
		store.RegisterReducers(
			FeatureReducer.Build(new PersonState("Hello", "World"))
				.On<UpdateFirstNameAction>((s, a) => s with { FirstName = a.FirstName })
				.On<UpdateLastNameAction>((s, a) => s with { LastName = a.LastName }));
		return store;
	}

	private sealed class FirstNameSideEffectFactory(Action onFirstName) : IEffectsFactory
	{
		public IEnumerable<Effect> Create() =>
		[
			EffectsFactory.Create(actions => actions
				.OfType<UpdateFirstNameAction>()
				.Do(_ => onFirstName()))
		];
	}
}
