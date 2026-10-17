using System.Reactive.Linq;
using MentorLake.Redux.Middleware;
using MentorLake.Redux.Reducers;

namespace MentorLake.Redux.Tests;

public record BatchCounterState(int Value);
public record BatchIncrementAction(int Amount);

[TestClass]
public class BatchingTests
{
	private static ReduxStore CreateStore()
	{
		var store = new ReduxStore();
		store.RegisterReducers(
			FeatureReducer.Build(new BatchCounterState(0))
				.On<BatchIncrementAction>((state, action) => state with { Value = state.Value + action.Amount }));
		return store;
	}

	[TestMethod]
	public void WithoutBatch_EmitsOneNotificationPerDispatch()
	{
		var store = CreateStore();
		var notifications = 0;
		store.Select(s => s.GetFeatureState<BatchCounterState>()).Skip(1).Subscribe(_ => notifications++);

		store.Dispatch(new BatchIncrementAction(1));
		store.Dispatch(new BatchIncrementAction(2));
		store.Dispatch(new BatchIncrementAction(3));

		Assert.AreEqual(3, notifications);
	}

	[TestMethod]
	public void BatchOfActions_CoalescesIntoOneNotification()
	{
		var store = CreateStore();
		var notifications = 0;
		store.Select(s => s.GetFeatureState<BatchCounterState>()).Skip(1).Subscribe(_ => notifications++);

		store.Dispatch(new BatchIncrementAction(1), new BatchIncrementAction(2), new BatchIncrementAction(3));

		Assert.AreEqual(1, notifications);
		Assert.AreEqual(6, store.State.GetFeatureState<BatchCounterState>().Value);
	}

	[TestMethod]
	public void Batch_AppliesAllActionsInOrder()
	{
		var store = CreateStore();

		store.Dispatch(new BatchIncrementAction(1), new BatchIncrementAction(2), new BatchIncrementAction(3));

		Assert.AreEqual(6, store.State.GetFeatureState<BatchCounterState>().Value);
	}

	[TestMethod]
	public void Batch_EmitsEveryActionOnActionStream()
	{
		var store = CreateStore();
		var actions = new List<object>();
		store.Actions.Subscribe(a => actions.Add(a));

		store.Dispatch(new BatchIncrementAction(1), new BatchIncrementAction(2));

		Assert.AreEqual(2, actions.Count);
	}

	[TestMethod]
	public void Batch_WithNoActions_EmitsNoNotification()
	{
		var store = CreateStore();
		var notifications = 0;
		store.Select(s => s.GetFeatureState<BatchCounterState>()).Skip(1).Subscribe(_ => notifications++);

		store.Dispatch();

		Assert.AreEqual(0, notifications);
	}

	[TestMethod]
	public void Batch_WithNoStateChange_EmitsNoNotification()
	{
		var store = CreateStore();
		var notifications = 0;
		store.Select(s => s.GetFeatureState<BatchCounterState>()).Skip(1).Subscribe(_ => notifications++);

		store.Dispatch(new object(), new object());

		Assert.AreEqual(0, notifications);
	}

	[TestMethod]
	public void Batch_AfterException_StoreRemainsUsable()
	{
		var store = new ReduxStore();
		store.UseMiddleware(ReduxMiddleware.Create((action, next) =>
		{
			if (action is BatchIncrementAction { Amount: 99 }) throw new InvalidOperationException("boom");
			next(action);
		}));
		store.RegisterReducers(
			FeatureReducer.Build(new BatchCounterState(0))
				.On<BatchIncrementAction>((state, action) => state with { Value = state.Value + action.Amount }));

		var notifications = 0;
		store.Select(s => s.GetFeatureState<BatchCounterState>()).Skip(1).Subscribe(_ => notifications++);

		try
		{
			store.Dispatch(new BatchIncrementAction(1), new BatchIncrementAction(99));
			Assert.Fail("caller should observe middleware exception");
		}
		catch (InvalidOperationException)
		{
		}

		store.Dispatch(new BatchIncrementAction(5));
		Assert.AreEqual(6, store.State.GetFeatureState<BatchCounterState>().Value);
	}
}
