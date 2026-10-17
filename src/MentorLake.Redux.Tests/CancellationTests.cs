using System.Reactive.Linq;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Thunks;

namespace MentorLake.Redux.Tests;

public record CancelTriggerAction(int Id);
public record CancelResultAction(int Id);

[TestClass]
public class CancellationTests
{
	private static async Task WaitUntil(Func<bool> condition, int timeoutMs = 2000)
	{
		var elapsed = 0;
		while (!condition() && elapsed < timeoutMs)
		{
			await Task.Delay(10);
			elapsed += 10;
		}

		if (!condition()) Assert.Fail("Condition not met within timeout.");
	}

	[TestMethod]
	public async Task Cancel_SignalsTokenAndDispatchesThunkCancelled()
	{
		var store = new ReduxStore();
		var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		CancellationToken token = default;

		var result = store.DispatchThunk(async api =>
		{
			token = api.CancellationToken;
			started.SetResult();
			await Task.Delay(Timeout.Infinite, api.CancellationToken);
		});

		var actions = new List<object>();
		result.Actions.Subscribe(a => actions.Add(a));

		await started.Task;
		result.Cancel();
		await WaitUntil(() => actions.Any(a => a is ThunkCancelled));

		Assert.IsTrue(token.IsCancellationRequested);
		Assert.IsFalse(actions.Any(a => a is ThunkFulfilled));
		Assert.IsFalse(actions.Any(a => a is ThunkRejected));
	}

	[TestMethod]
	public async Task Cancel_ToTask_ThrowsOperationCanceled()
	{
		var store = new ReduxStore();
		var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

		var result = store.DispatchThunk(async api =>
		{
			started.SetResult();
			await Task.Delay(Timeout.Infinite, api.CancellationToken);
		});

		var task = result.ToTask();
		await started.Task;
		result.Cancel();

		await Assert.ThrowsAsync<OperationCanceledException>(() => task);
	}

	[TestMethod]
	public async Task DispatchThunkLatest_CancelsPreviousInSameGroup()
	{
		var store = new ReduxStore();
		var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

		var first = store.DispatchThunkLatest("search", async api =>
		{
			firstStarted.SetResult();
			await Task.Delay(Timeout.Infinite, api.CancellationToken);
		});
		var firstActions = new List<object>();
		first.Actions.Subscribe(a => firstActions.Add(a));
		await firstStarted.Task;

		var second = store.DispatchThunkLatest("search", async _ => await Task.Delay(10));
		await second.ToTask();

		await WaitUntil(() => firstActions.Any(a => a is ThunkCancelled));
	}

	[TestMethod]
	public async Task DispatchThunkLatest_DifferentGroups_DoNotCancelEachOther()
	{
		var store = new ReduxStore();

		var first = store.DispatchThunkLatest("group-a", async _ => await Task.Delay(50));
		var second = store.DispatchThunkLatest("group-b", async _ => await Task.Delay(10));

		await first.ToTask();
		await second.ToTask();
	}

	[TestMethod]
	public async Task Dispatcher_DispatchLatest_CancelsPreviousInSameGroup()
	{
		var store = new ReduxStore();
		var dispatcher = store.Using<ThunkApi>();
		var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

		var first = dispatcher.DispatchLatest("search", async api =>
		{
			firstStarted.SetResult();
			await Task.Delay(Timeout.Infinite, api.CancellationToken);
		});
		var firstActions = new List<object>();
		first.Actions.Subscribe(a => firstActions.Add(a));
		await firstStarted.Task;

		var second = dispatcher.DispatchLatest("search", async _ => await Task.Delay(10));
		await second.ToTask();

		await WaitUntil(() => firstActions.Any(a => a is ThunkCancelled));
	}

	[TestMethod]
	public async Task Dispatcher_DispatchLatest_DifferentGroups_DoNotCancelEachOther()
	{
		var store = new ReduxStore();
		var dispatcher = store.Using<ThunkApi>();

		var first = dispatcher.DispatchLatest("group-a", async _ => await Task.Delay(50));
		var second = dispatcher.DispatchLatest("group-b", async _ => await Task.Delay(10));

		await first.ToTask();
		await second.ToTask();
	}

	[TestMethod]
	public async Task DispatchThunk_CallerToken_CancelsThunk()
	{
		var store = new ReduxStore();
		var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		using var caller = new CancellationTokenSource();
		CancellationToken token = default;

		var result = store.DispatchThunk(async api =>
		{
			token = api.CancellationToken;
			started.SetResult();
			await Task.Delay(Timeout.Infinite, api.CancellationToken);
		}, caller.Token);

		var actions = new List<object>();
		result.Actions.Subscribe(a => actions.Add(a));

		await started.Task;
		caller.Cancel();
		await WaitUntil(() => actions.Any(a => a is ThunkCancelled));

		Assert.IsTrue(token.IsCancellationRequested);
		Assert.IsFalse(actions.Any(a => a is ThunkFulfilled));
	}

	[TestMethod]
	public async Task DispatchThunk_CallerToken_CancelStillWorks()
	{
		var store = new ReduxStore();
		var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		using var caller = new CancellationTokenSource();

		var result = store.DispatchThunk(async api =>
		{
			started.SetResult();
			await Task.Delay(Timeout.Infinite, api.CancellationToken);
		}, caller.Token);

		var actions = new List<object>();
		result.Actions.Subscribe(a => actions.Add(a));

		await started.Task;
		result.Cancel();
		await WaitUntil(() => actions.Any(a => a is ThunkCancelled));
	}

	[TestMethod]
	public async Task DispatchThunkLatest_CallerToken_CancelsThunk()
	{
		var store = new ReduxStore();
		var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		using var caller = new CancellationTokenSource();

		var result = store.DispatchThunkLatest("search", async api =>
		{
			started.SetResult();
			await Task.Delay(Timeout.Infinite, api.CancellationToken);
		}, caller.Token);

		var actions = new List<object>();
		result.Actions.Subscribe(a => actions.Add(a));

		await started.Task;
		caller.Cancel();
		await WaitUntil(() => actions.Any(a => a is ThunkCancelled));
	}

	[TestMethod]
	public async Task DispatchThunkLatest_SubscribedMultipleTimes_RunsThunkOnce()
	{
		var store = new ReduxStore();
		var executions = 0;

		var result = store.DispatchThunkLatest("group", async _ =>
		{
			Interlocked.Increment(ref executions);
			await Task.Delay(10);
		});

		result.Actions.Subscribe(_ => { });
		result.Actions.Subscribe(_ => { });
		await result.ToTask();

		Assert.AreEqual(1, executions);
	}

	[TestMethod]
	public async Task EffectTakeLatest_CancelsPreviousWork()
	{
		var store = new ReduxStore();
		var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		CancellationToken firstToken = default;
		var executions = 0;

		var effect = Effect.Build()
			.On<CancelTriggerAction>()
			.TakeLatest()
			.Handle(action => Observable.FromAsync<int>(async ct =>
			{
				var id = Interlocked.Increment(ref executions);
				if (id == 1)
				{
					firstToken = ct;
					firstStarted.SetResult();
					await Task.Delay(Timeout.Infinite, ct);
				}

				return id;
			}).Select(id => (object) new CancelResultAction(id)))
			.DispatchResults()
			.Build();

		store.RegisterEffects(effect);
		var results = new List<int>();
		store.Actions.Subscribe(a =>
		{
			if (a is CancelResultAction r) results.Add(r.Id);
		});

		store.Dispatch(new CancelTriggerAction(1));
		await firstStarted.Task;

		store.Dispatch(new CancelTriggerAction(2));
		await WaitUntil(() => results.Count == 1);

		Assert.IsTrue(firstToken.IsCancellationRequested);
		CollectionAssert.AreEqual(new[] { 2 }, results);

		await Task.Delay(50);
		Assert.AreEqual(1, results.Count);
	}

	[TestMethod]
	public async Task EffectTakeEvery_RunsAllConcurrently()
	{
		var store = new ReduxStore();
		var executions = 0;

		var effect = Effect.Build()
			.On<CancelTriggerAction>()
			.TakeEvery()
			.Handle(action => Observable.FromAsync<int>(async ct =>
			{
				var id = Interlocked.Increment(ref executions);
				await Task.Delay(30, ct);
				return id;
			}).Select(id => (object) new CancelResultAction(id)))
			.DispatchResults()
			.Build();

		store.RegisterEffects(effect);
		var results = new List<int>();
		store.Actions.Subscribe(a =>
		{
			if (a is CancelResultAction r) results.Add(r.Id);
		});

		store.Dispatch(new CancelTriggerAction(1));
		store.Dispatch(new CancelTriggerAction(2));

		await WaitUntil(() => results.Count == 2);
		Assert.AreEqual(2, executions);
	}

	[TestMethod]
	public async Task EffectExhaust_IgnoresTriggersWhileRunning()
	{
		var store = new ReduxStore();
		var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var executions = 0;

		var effect = Effect.Build()
			.On<CancelTriggerAction>()
			.Exhaust()
			.Handle(action => Observable.FromAsync<int>(async ct =>
			{
				var id = Interlocked.Increment(ref executions);
				if (id == 1)
				{
					firstStarted.SetResult();
					await release.Task;
				}

				return id;
			}).Select(id => (object) new CancelResultAction(id)))
			.DispatchResults()
			.Build();

		store.RegisterEffects(effect);
		var results = new List<int>();
		store.Actions.Subscribe(a =>
		{
			if (a is CancelResultAction r) results.Add(r.Id);
		});

		store.Dispatch(new CancelTriggerAction(1));
		await firstStarted.Task;

		store.Dispatch(new CancelTriggerAction(2));
		await Task.Delay(50);
		Assert.AreEqual(0, results.Count);

		release.SetResult();
		await WaitUntil(() => results.Count == 1);

		store.Dispatch(new CancelTriggerAction(3));
		await WaitUntil(() => results.Count == 2);
		CollectionAssert.AreEqual(new[] { 1, 2 }, results);
		Assert.AreEqual(2, executions);
	}

	[TestMethod]
	public async Task EffectDebounce_CollapsesRapidTriggers()
	{
		var store = new ReduxStore();
		var executions = 0;

		var effect = Effect.Build()
			.On<CancelTriggerAction>()
			.Debounce(TimeSpan.FromMilliseconds(150))
			.TakeLatest()
			.Handle(action => Observable.FromAsync<int>(ct =>
			{
				return Task.FromResult(Interlocked.Increment(ref executions));
			}).Select(id => (object) new CancelResultAction(id)))
			.DispatchResults()
			.Build();

		store.RegisterEffects(effect);
		var results = new List<int>();
		store.Actions.Subscribe(a =>
		{
			if (a is CancelResultAction r) results.Add(r.Id);
		});

		store.Dispatch(new CancelTriggerAction(1));
		await Task.Delay(30);
		store.Dispatch(new CancelTriggerAction(2));

		await WaitUntil(() => results.Count == 1, 3000);
		await Task.Delay(200);

		Assert.AreEqual(1, executions);
	}
}
