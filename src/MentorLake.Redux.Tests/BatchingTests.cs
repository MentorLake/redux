using System.Reactive.Linq;
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
	public void Batch_CoalescesStateNotifications()
	{
		var store = CreateStore();
		var notifications = 0;
		store.Select(s => s.GetFeatureState<BatchCounterState>()).Skip(1).Subscribe(_ => notifications++);

		store.Batch(() =>
		{
			store.Dispatch(new BatchIncrementAction(1));
			store.Dispatch(new BatchIncrementAction(2));
			store.Dispatch(new BatchIncrementAction(3));
		});

		Assert.AreEqual(1, notifications);
		Assert.AreEqual(6, store.State.GetFeatureState<BatchCounterState>().Value);
	}

	[TestMethod]
	public void Batch_UpdatesStateSynchronously()
	{
		var store = CreateStore();
		store.Batch(() =>
		{
			store.Dispatch(new BatchIncrementAction(1));
			Assert.AreEqual(1, store.State.GetFeatureState<BatchCounterState>().Value);
			store.Dispatch(new BatchIncrementAction(1));
			Assert.AreEqual(2, store.State.GetFeatureState<BatchCounterState>().Value);
		});
	}

	[TestMethod]
	public void Batch_EmitsEveryActionOnActionStream()
	{
		var store = CreateStore();
		var actions = new List<object>();
		store.Actions.Subscribe(a => actions.Add(a));

		store.Batch(() =>
		{
			store.Dispatch(new BatchIncrementAction(1));
			store.Dispatch(new BatchIncrementAction(2));
		});

		Assert.AreEqual(2, actions.Count);
	}

	[TestMethod]
	public void NestedBatch_CoalescesIntoOutermost()
	{
		var store = CreateStore();
		var notifications = 0;
		store.Select(s => s.GetFeatureState<BatchCounterState>()).Skip(1).Subscribe(_ => notifications++);

		store.Batch(() =>
		{
			store.Dispatch(new BatchIncrementAction(1));
			store.Batch(() => store.Dispatch(new BatchIncrementAction(2)));
			Assert.AreEqual(0, notifications);
		});

		Assert.AreEqual(1, notifications);
	}

	[TestMethod]
	public void Batch_DispatchesAfterException_StillFlushes()
	{
		var store = CreateStore();
		var notifications = 0;
		store.Select(s => s.GetFeatureState<BatchCounterState>()).Skip(1).Subscribe(_ => notifications++);

		try
		{
			store.Batch(() =>
			{
				store.Dispatch(new BatchIncrementAction(1));
				throw new InvalidOperationException("boom");
			});
		}
		catch (InvalidOperationException)
		{
		}

		Assert.AreEqual(1, notifications);
		store.Dispatch(new BatchIncrementAction(1));
		Assert.AreEqual(2, notifications);
	}

	[TestMethod]
	public void Batch_WithNoDispatches_EmitsNoNotification()
	{
		var store = CreateStore();
		var notifications = 0;
		store.Select(s => s.GetFeatureState<BatchCounterState>()).Skip(1).Subscribe(_ => notifications++);

		store.Batch(() => { });

		Assert.AreEqual(0, notifications);
	}
}
