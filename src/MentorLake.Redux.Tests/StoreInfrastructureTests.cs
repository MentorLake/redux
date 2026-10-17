using System.Diagnostics;
using System.Reflection;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MentorLake.Redux.Middleware;
using MentorLake.Redux.Reducers;

namespace MentorLake.Redux.Tests;

public record StoreInfraState(int Value);
public record StoreInfraTick;
public record StoreInfraBoom;

[TestClass]
public class StoreInfrastructureTests
{
	private static ReduxStore CreateStore()
	{
		var store = new ReduxStore();
		store.RegisterReducers(
			FeatureReducer.Build(new StoreInfraState(0))
				.On<StoreInfraTick>((state, _) => state with { Value = state.Value + 1 }));
		return store;
	}

	private static ReduxStore CreateExplodingStore()
	{
		var store = new ReduxStore();
		store.UseMiddleware(ReduxMiddleware.Create((action, next) =>
		{
			if (action is StoreInfraBoom) throw new InvalidOperationException("boom");
			next(action);
		}));
		store.RegisterReducers(
			FeatureReducer.Build(new StoreInfraState(0))
				.On<StoreInfraTick>((state, _) => state with { Value = state.Value + 1 }));
		return store;
	}

	[TestMethod]
	public void Dispatch_SingleAction_IsSynchronousForCaller()
	{
		var store = CreateStore();
		store.Dispatch(new StoreInfraTick());
		Assert.AreEqual(1, store.State.GetFeatureState<StoreInfraState>().Value);
	}

	[TestMethod]
	public void Dispatch_NullSingleAction_IsNoOp()
	{
		var store = CreateStore();
		store.Dispatch((object)null);
		Assert.AreEqual(0, store.State.GetFeatureState<StoreInfraState>().Value);
	}

	[TestMethod]
	public void Dispatch_NullArray_IsNoOp()
	{
		var store = CreateStore();
		store.Dispatch((object[])null);
		Assert.AreEqual(0, store.State.GetFeatureState<StoreInfraState>().Value);
	}

	[TestMethod]
	public void Dispatch_EmptyArray_IsNoOp()
	{
		var store = CreateStore();
		store.Dispatch();
		Assert.AreEqual(0, store.State.GetFeatureState<StoreInfraState>().Value);
	}

	[TestMethod]
	public void Dispatch_ArrayWithNulls_IgnoresNullsAndAppliesRest()
	{
		var store = CreateStore();
		store.Dispatch(new StoreInfraTick(), null, new StoreInfraTick());
		Assert.AreEqual(2, store.State.GetFeatureState<StoreInfraState>().Value);
	}

	[TestMethod]
	public void Dispatch_ArrayOfOnlyNulls_IsNoOp()
	{
		var store = CreateStore();
		var notifications = 0;
		store.Select(s => s.GetFeatureState<StoreInfraState>()).Skip(1).Subscribe(_ => notifications++);

		store.Dispatch(null, null);

		Assert.AreEqual(0, store.State.GetFeatureState<StoreInfraState>().Value);
		Assert.AreEqual(0, notifications);
	}

	[TestMethod]
	public void Dispatch_MiddlewareShortCircuit_NoStateChangeNoNotification()
	{
		var store = CreateStore();
		store.UseMiddleware(ReduxMiddleware.Create((action, next) =>
		{
			if (action is StoreInfraTick) return;
			next(action);
		}));

		var notifications = 0;
		store.Select(s => s.GetFeatureState<StoreInfraState>()).Skip(1).Subscribe(_ => notifications++);

		store.Dispatch(new StoreInfraTick());

		Assert.AreEqual(0, store.State.GetFeatureState<StoreInfraState>().Value);
		Assert.AreEqual(0, notifications);
	}

	[TestMethod]
	public void Dispatch_ReducerReturnsSameState_NoNotification()
	{
		var store = new ReduxStore();
		store.RegisterReducers(
			FeatureReducer.Build(new StoreInfraState(0))
				.On<StoreInfraTick>((state, _) => state));
		var notifications = 0;
		store.Select(s => s.GetFeatureState<StoreInfraState>()).Skip(1).Subscribe(_ => notifications++);

		store.Dispatch(new StoreInfraTick());

		Assert.AreEqual(0, notifications);
	}

	[TestMethod]
	public void MidBatchFailure_PartialStateIsNotifiedAndCallerFaults()
	{
		var store = CreateExplodingStore();
		var notifications = 0;
		store.Select(s => s.GetFeatureState<StoreInfraState>()).Skip(1).Subscribe(_ => notifications++);

		var threw = false;
		try
		{
			store.Dispatch(new StoreInfraTick(), new StoreInfraBoom(), new StoreInfraTick());
		}
		catch (InvalidOperationException)
		{
			threw = true;
		}

		Assert.IsTrue(threw);
		Assert.AreEqual(1, store.State.GetFeatureState<StoreInfraState>().Value);
		Assert.AreEqual(1, notifications);
	}

	[TestMethod]
	public void MidBatchFailure_NoStateChangeYet_NoNotification()
	{
		var store = CreateExplodingStore();
		var notifications = 0;
		store.Select(s => s.GetFeatureState<StoreInfraState>()).Skip(1).Subscribe(_ => notifications++);

		try
		{
			store.Dispatch(new StoreInfraBoom(), new StoreInfraTick());
		}
		catch (InvalidOperationException)
		{
		}

		Assert.AreEqual(0, store.State.GetFeatureState<StoreInfraState>().Value);
		Assert.AreEqual(0, notifications);
	}

	[TestMethod]
	public void ProcessorSurvives_RepeatedFailures()
	{
		var store = CreateExplodingStore();

		for (var i = 0; i < 100; i++)
		{
			try { store.Dispatch(new StoreInfraBoom()); }
			catch (InvalidOperationException) { }
		}

		store.Dispatch(new StoreInfraTick());
		Assert.AreEqual(1, store.State.GetFeatureState<StoreInfraState>().Value);
	}

	[TestMethod]
	public void ErrorsObservable_ReceivesDispatchFailures()
	{
		var store = CreateExplodingStore();
		var errors = new List<Exception>();
		store.Errors.Subscribe(errors.Add);

		try { store.Dispatch(new StoreInfraBoom()); }
		catch (InvalidOperationException) { }

		Assert.AreEqual(1, errors.Count);
		Assert.AreEqual("boom", errors[0].Message);
	}

	[TestMethod]
	public void UnobservedError_DoesNotCrashStore()
	{
		var store = CreateExplodingStore();

		try { store.Dispatch(new StoreInfraBoom()); }
		catch (InvalidOperationException) { }

		store.Dispatch(new StoreInfraTick());
		Assert.AreEqual(1, store.State.GetFeatureState<StoreInfraState>().Value);
	}

	[TestMethod]
	public void DisposeAsync_BlocksSubsequentDispatch()
	{
		var store = CreateStore();
		store.DisposeAsync().AsTask().GetAwaiter().GetResult();

		Assert.Throws<ObjectDisposedException>(() => store.Dispatch(new StoreInfraTick()));
	}

	[TestMethod]
	public async Task DisposeAsync_IsIdempotent()
	{
		var store = CreateStore();
		await store.DisposeAsync();
		await store.DisposeAsync();
	}

	[TestMethod]
	public async Task DisposeAsync_ConcurrentCallsBothComplete()
	{
		var store = CreateStore();
		var first = store.DisposeAsync().AsTask();
		var second = store.DisposeAsync().AsTask();

		await Task.WhenAll(first, second);

		Assert.IsFalse(first.IsFaulted);
		Assert.IsFalse(second.IsFaulted);
	}

	[TestMethod]
	public async Task DisposeAsync_WaitsForInFlightDispatch()
	{
		var store = CreateStore();
		store.UseMiddleware(ReduxMiddleware.Create((action, next) =>
		{
			if (action is StoreInfraTick) Thread.Sleep(1000);
			next(action);
		}));

		var blocked = Task.Run(() => store.Dispatch(new StoreInfraTick()));
		await Task.Delay(100);

		await store.DisposeAsync();
		await blocked;

		Assert.AreEqual(1, store.State.GetFeatureState<StoreInfraState>().Value);
	}

	[TestMethod]
	public async Task DisposeAsync_DrainsQueuedActions()
	{
		var store = CreateStore();
		store.Dispatch(Enumerable.Repeat<object>(new StoreInfraTick(), 200000).ToArray());

		var sw = Stopwatch.StartNew();
		await store.DisposeAsync();

		Assert.AreEqual(200000, store.State.GetFeatureState<StoreInfraState>().Value);
	}

	[TestMethod]
	public async Task DisposeAsync_DisposesUnderlyingSubjects()
	{
		var store = CreateStore();
		await store.DisposeAsync();

		var actions = store.GetType().GetField("_actionDispatcher", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(store);
		var errors = store.GetType().GetField("_errorSubject", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(store);

		await Assert.ThrowsAsync<ObjectDisposedException>(async () => await ((Subject<object>)actions!).FirstAsync());
		await Assert.ThrowsAsync<ObjectDisposedException>(async () => await ((Subject<Exception>)errors!).FirstAsync());
	}

	[TestMethod]
	public async Task Dispatch_AfterDispose_ThrowsObjectDisposed()
	{
		var store = CreateStore();
		await store.DisposeAsync();

		Assert.Throws<ObjectDisposedException>(() => store.Dispatch(new StoreInfraTick()));
	}

	[TestMethod]
	public void ConcurrentDispatch_NoLostUpdates()
	{
		var store = CreateStore();
		const int threads = 8;
		const int perThread = 1000;

		Parallel.For(0, threads, _ =>
		{
			for (var i = 0; i < perThread; i++)
			{
				store.Dispatch(new StoreInfraTick());
			}
		});

		Assert.AreEqual(threads * perThread, store.State.GetFeatureState<StoreInfraState>().Value);
	}

	[TestMethod]
	public void ConcurrentBatches_AreAppliedWhole()
	{
		var store = CreateStore();
		var notifications = 0;
		var notificationValues = new List<int>();
		var recordLock = new object();
		store.Select(s => s.GetFeatureState<StoreInfraState>().Value).Skip(1).Subscribe(v =>
		{
			lock (recordLock)
			{
				notificationValues.Add(v);
				notifications++;
			}
		});

		const int threads = 4;
		const int batchSize = 250;

		Parallel.For(0, threads, _ =>
		{
			store.Dispatch(Enumerable.Repeat<object>(new StoreInfraTick(), batchSize).ToArray());
		});

		Assert.AreEqual(threads * batchSize, store.State.GetFeatureState<StoreInfraState>().Value);
		Assert.AreEqual(threads, notifications);

		lock (recordLock)
		{
			var sorted = notificationValues.OrderBy(v => v).ToList();
			CollectionAssert.AreEqual(sorted, notificationValues);
			for (var i = 0; i < sorted.Count; i++)
			{
				Assert.AreEqual((i + 1) * batchSize, sorted[i]);
			}
		}
	}

	[TestMethod]
	public void ReentrantDispatch_FromMiddleware_IsAppliedInline()
	{
		var store = CreateStore();
		store.UseMiddleware(ReduxMiddleware.Create((action, next) =>
		{
			next(action);
			if (action is StoreInfraTick { } && store.State.GetFeatureState<StoreInfraState>().Value == 1)
			{
				store.Dispatch(new StoreInfraTick());
			}
		}));

		store.Dispatch(new StoreInfraTick());

		Assert.AreEqual(2, store.State.GetFeatureState<StoreInfraState>().Value);
	}

	[TestMethod]
	public void Dispatch_AfterProcessorHandledFailure_StoreRemainsUsable()
	{
		var store = CreateExplodingStore();

		try { store.Dispatch(new StoreInfraBoom()); }
		catch (InvalidOperationException) { }

		store.Dispatch(new StoreInfraTick(), new StoreInfraTick());
		Assert.AreEqual(2, store.State.GetFeatureState<StoreInfraState>().Value);
	}
}
