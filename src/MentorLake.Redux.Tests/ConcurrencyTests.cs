using MentorLake.Redux.Reducers;

namespace MentorLake.Redux.Tests;

public record RaceCounterState(int Value);
public record RaceIncrementAction;

[TestClass]
public class ConcurrencyTests
{
	private static ReduxStore CreateStore()
	{
		var store = new ReduxStore();
		store.RegisterReducers(
			FeatureReducer.Build(new RaceCounterState(0))
				.On<RaceIncrementAction>((state, _) => state with { Value = state.Value + 1 }));
		return store;
	}

	[TestMethod]
	public async Task ConcurrentDispatch_NoLostUpdates()
	{
		var store = CreateStore();
		const int threads = 8;
		const int perThread = 1000;

		Parallel.For(0, threads, _ =>
		{
			for (var i = 0; i < perThread; i++)
			{
				store.Dispatch(new RaceIncrementAction());
			}
		});
		Assert.AreEqual(threads * perThread, store.State.GetFeatureState<RaceCounterState>().Value);
	}

	[TestMethod]
	public async Task ConcurrentDispatchAndWait_AllComplete()
	{
		var store = CreateStore();
		const int tasks = 50;
		const int perTask = 100;

		var workers = Enumerable.Range(0, tasks).Select(async _ =>
		{
			for (var i = 0; i < perTask; i++)
			{
				store.Dispatch(new RaceIncrementAction());
			}
		}).ToArray();

		await Task.WhenAll(workers);

		Assert.AreEqual(tasks * perTask, store.State.GetFeatureState<RaceCounterState>().Value);
	}

	[TestMethod]
	public async Task ConcurrentBatchAndDispatch_StateRemainsConsistent()
	{
		var store = CreateStore();
		const int threads = 4;
		const int perThread = 500;

		Parallel.For(0, threads, i =>
		{
			if (i % 2 == 0)
			{
				for (var n = 0; n < perThread; n++)
				{
					store.Dispatch(new RaceIncrementAction());
				}
			}
			else
			{
				store.Dispatch(Enumerable.Repeat<object>(new RaceIncrementAction(), perThread).ToArray());
			}
		});

		//await store.WaitUntilIdleAsync();

		Assert.AreEqual(threads * perThread, store.State.GetFeatureState<RaceCounterState>().Value);
	}
}
