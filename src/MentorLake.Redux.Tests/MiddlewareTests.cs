using MentorLake.Redux.Middleware;
using MentorLake.Redux.Reducers;

namespace MentorLake.Redux.Tests;

public record MwCounterState(int Value);
public record MwIncrementAction(int Amount);
public record MwRewrittenAction(int Amount);

[TestClass]
public class MiddlewareTests
{
	private static ReduxStore CreateStore()
	{
		var store = new ReduxStore();
		store.RegisterReducers(
			FeatureReducer.Build(new MwCounterState(0))
				.On<MwIncrementAction>((state, action) => state with { Value = state.Value + action.Amount })
				.On<MwRewrittenAction>((state, action) => state with { Value = state.Value - action.Amount }));
		return store;
	}

	[TestMethod]
	public void Middleware_ReceivesAction_AndNextAppliesReducers()
	{
		var store = CreateStore();
		var seen = new List<object>();
		store.UseMiddleware(ReduxMiddleware.Create((action, next) =>
		{
			seen.Add(action);
			next(action);
		}));

		store.Dispatch(new MwIncrementAction(5));

		CollectionAssert.AreEqual(new object[] { new MwIncrementAction(5) }, seen);
		Assert.AreEqual(5, store.State.GetFeatureState<MwCounterState>().Value);
	}

	[TestMethod]
	public void Middleware_CanShortCircuit()
	{
		var store = CreateStore();
		store.UseMiddleware(ReduxMiddleware.Create((action, next) =>
		{
			if (action is MwIncrementAction { Amount: < 0 }) return;
			next(action);
		}));

		var dispatched = new List<object>();
		store.Actions.Subscribe(a => dispatched.Add(a));

		store.Dispatch(new MwIncrementAction(-1));
		Assert.AreEqual(0, store.State.GetFeatureState<MwCounterState>().Value);
		Assert.AreEqual(0, dispatched.Count);

		store.Dispatch(new MwIncrementAction(2));
		Assert.AreEqual(2, store.State.GetFeatureState<MwCounterState>().Value);
	}

	[TestMethod]
	public void Middleware_CanRewriteAction()
	{
		var store = CreateStore();
		store.UseMiddleware(ReduxMiddleware.Create((action, next) =>
		{
			next(action is MwIncrementAction inc ? new MwRewrittenAction(inc.Amount) : action);
		}));

		store.Dispatch(new MwIncrementAction(3));

		Assert.AreEqual(-3, store.State.GetFeatureState<MwCounterState>().Value);
	}

	[TestMethod]
	public void Middleware_RunsInRegistrationOrder()
	{
		var store = CreateStore();
		var order = new List<string>();
		store.UseMiddleware(ReduxMiddleware.Create((action, next) =>
		{
			order.Add("outer-before");
			next(action);
			order.Add("outer-after");
		}));
		store.UseMiddleware(ReduxMiddleware.Create((action, next) =>
		{
			order.Add("inner");
			next(action);
		}));

		store.Dispatch(new MwIncrementAction(1));

		CollectionAssert.AreEqual(new[] { "outer-before", "inner", "outer-after" }, order);
	}

	[TestMethod]
	public void Middleware_Initialize_ReceivesStore()
	{
		var store = CreateStore();
		ReduxStore initializedWith = null;
		store.UseMiddleware(ReduxMiddleware.Create(
			(action, next) => next(action),
			s => initializedWith = s));

		Assert.AreSame(store, initializedWith);
	}

	[TestMethod]
	public void Middleware_CanDispatchAdditionalActions()
	{
		var store = CreateStore();
		store.UseMiddleware(ReduxMiddleware.Create((action, next) =>
		{
			next(action);
			if (action is MwIncrementAction { Amount: 1 }) store.Dispatch(new MwIncrementAction(10));
		}));

		store.Dispatch(new MwIncrementAction(1));

		Assert.AreEqual(11, store.State.GetFeatureState<MwCounterState>().Value);
	}
}
