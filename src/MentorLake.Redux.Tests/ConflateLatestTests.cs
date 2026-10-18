using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MentorLake.Redux.Tests;

[TestClass]
public class ConflateLatestTests
{
	[TestMethod]
	public async Task ConflateLatestForwardsSingleValue()
	{
		var scheduler = new EventLoopScheduler();
		var source = new Subject<int>();
		var results = new List<int>();
		using var sub = source.ConflateLatest(scheduler).Subscribe(results.Add);

		source.OnNext(1);
		await WaitFor(() => results.Count == 1);

		CollectionAssert.AreEqual(new[] { 1 }, results);
	}

	[TestMethod]
	public async Task ConflateLatestCollapsesBurstToLatest()
	{
		var scheduler = new EventLoopScheduler();
		var source = new Subject<int>();
		var results = new List<int>();
		var firstDeliveryStarted = new ManualResetEventSlim(false);
		var releaseFirstDelivery = new ManualResetEventSlim(false);
		using var sub = source.ConflateLatest(scheduler).Subscribe(val =>
		{
			lock (results) { results.Add(val); }
			if (val == 1)
			{
				firstDeliveryStarted.Set();
				releaseFirstDelivery.Wait();
			}
		});

		source.OnNext(1);
		Assert.IsTrue(firstDeliveryStarted.Wait(TimeSpan.FromSeconds(2)), "First delivery never started.");

		// 2 and 3 arrive while delivery of 1 is in flight, so 2 is guaranteed to be dropped.
		source.OnNext(2);
		source.OnNext(3);
		releaseFirstDelivery.Set();

		await WaitFor(() => { lock (results) { return results.Count == 2; } });
		lock (results) { CollectionAssert.AreEqual(new[] { 1, 3 }, results); }
	}

	[TestMethod]
	public async Task ConflateLatestEmitsNewValueAfterDrain()
	{
		var scheduler = new EventLoopScheduler();
		var source = new Subject<int>();
		var results = new List<int>();
		using var sub = source.ConflateLatest(scheduler).Subscribe(results.Add);

		source.OnNext(1);
		await WaitFor(() => results.Count == 1);
		source.OnNext(2);
		await WaitFor(() => results.Count == 2);

		CollectionAssert.AreEqual(new[] { 1, 2 }, results);
	}

	[TestMethod]
	public async Task ConflateLatestForwardsError()
	{
		var scheduler = new EventLoopScheduler();
		var source = new Subject<int>();
		var error = new TestException();
		Exception caught = null;
		using var sub = source.ConflateLatest(scheduler).Subscribe(_ => { }, ex => caught = ex);

		source.OnNext(1);
		await Task.Delay(50);
		source.OnError(error);
		await WaitFor(() => caught != null);

		Assert.AreSame(error, caught);
	}

	[TestMethod]
	public async Task ConflateLatestCompletesAfterPendingValue()
	{
		var scheduler = new EventLoopScheduler();
		var source = new Subject<int>();
		var results = new List<int>();
		var completed = false;
		using var sub = source.ConflateLatest(scheduler).Subscribe(results.Add, () => completed = true);

		source.OnNext(1);
		source.OnNext(2);
		source.OnCompleted();
		await WaitFor(() => completed);

		Assert.IsTrue(completed);
		Assert.AreEqual(2, results.Last());
	}

	[TestMethod]
	public async Task ConflateLatestDeliversFinalValueFromTimerSource()
	{
		var scheduler = new EventLoopScheduler();
		var results = new List<long>();
		var completed = false;
		using var sub = Observable.Timer(TimeSpan.FromMilliseconds(50))
			.ConflateLatest(scheduler)
			.Subscribe(results.Add, () => completed = true);

		await WaitFor(() => completed);

		Assert.IsTrue(completed);
		CollectionAssert.AreEqual(new[] { 0L }, results);
	}

	[TestMethod]
	public async Task ConflateLatestDeliversValueEmittedOnForeignThreadBeforeCompletion()
	{
		var scheduler = new EventLoopScheduler();
		var source = new Subject<int>();
		var results = new List<int>();
		var completed = false;
		using var sub = source.ConflateLatest(scheduler).Subscribe(results.Add, () => completed = true);

		var emitter = new Thread(() =>
		{
			source.OnNext(42);
			source.OnCompleted();
		});
		emitter.Start();
		emitter.Join();
		await WaitFor(() => completed);

		Assert.IsTrue(completed);
		CollectionAssert.AreEqual(new[] { 42 }, results);
	}

	[TestMethod]
	public async Task ConflateLatestStopsEmittingAfterDispose()
	{
		var scheduler = new EventLoopScheduler();
		var source = new Subject<int>();
		var results = new List<int>();
		var sub = source.ConflateLatest(scheduler).Subscribe(results.Add);

		source.OnNext(1);
		await WaitFor(() => results.Count == 1);
		sub.Dispose();
		source.OnNext(2);

		await Task.Delay(100);
		CollectionAssert.AreEqual(new[] { 1 }, results);
	}

	[TestMethod]
	public async Task ConflateLatestIsThreadSafeUnderConcurrentProducers()
	{
		var scheduler = new EventLoopScheduler();
		var source = new Subject<int>();
		var emitted = new List<int>();
		var lostUpdate = false;
		using var sub = source
			.ConflateLatest(scheduler)
			.Subscribe(val =>
			{
				var max = emitted.Count == 0 ? int.MinValue : emitted.Max();
				if (val < max) { lostUpdate = true; }
				emitted.Add(val);
			});

		var counter = 0;
		var churnDone = false;
		var barrier = new Barrier(2);
		var producer = Task.Run(() =>
		{
			barrier.SignalAndWait();
			for (var i = 0; i < 4000; i++)
			{
				source.OnNext(Interlocked.Increment(ref counter));
			}
		});
		var churn = Task.Run(() =>
		{
			barrier.SignalAndWait();
			while (!Volatile.Read(ref churnDone))
			{
				source.OnNext(Interlocked.Increment(ref counter));
				Thread.Sleep(1);
			}
		});

		await producer;
		await WaitFor(() => emitted.Any(v => v <= 4000));
		Volatile.Write(ref churnDone, true);
		await churn;

		Assert.IsFalse(lostUpdate);
	}

	[TestMethod]
	public async Task ConflateLatestDeliversOnSchedulerThread()
	{
		var scheduler = new EventLoopScheduler(t => new Thread(t) { Name = "conflate-test", IsBackground = true });
		var source = new Subject<int>();
		var deliveryThreads = new List<string>();
		using var sub = source.ConflateLatest(scheduler).Subscribe(_ => deliveryThreads.Add(Thread.CurrentThread.Name));

		source.OnNext(1);
		await WaitFor(() => deliveryThreads.Count == 1);
		source.OnNext(2);
		await WaitFor(() => deliveryThreads.Count == 2);

		CollectionAssert.AreEqual(new[] { "conflate-test", "conflate-test" }, deliveryThreads);
	}

	[TestMethod]
	public async Task ConflateLatestUsesPoolThreadsUnderTaskPoolScheduler()
	{
		var source = new Subject<int>();
		var threads = new List<Thread>();
		var allPool = true;
		using var sub = source.ConflateLatest(TaskPoolScheduler.Default).Subscribe(_ =>
		{
			lock (threads)
			{
				threads.Add(Thread.CurrentThread);
				if (!Thread.CurrentThread.IsThreadPoolThread) { allPool = false; }
			}
		});

		var producer = Task.Run(() =>
		{
			for (var i = 0; i < 200; i++) { source.OnNext(i); }
		});

		await WaitFor(() => threads.Count >= 2);
		await producer;

		Assert.IsTrue(allPool);
		Assert.IsTrue(threads.Count >= 2);
	}

	[TestMethod]
	public async Task ConflateLatestNeverDeliversConcurrently()
	{
		var source = new Subject<int>();
		var active = 0;
		var overlap = false;
		using var sub = source.ConflateLatest(TaskPoolScheduler.Default).Subscribe(_ =>
		{
			if (Interlocked.Increment(ref active) > 1) { overlap = true; }
			Thread.Sleep(1);
			Interlocked.Decrement(ref active);
		});

		var producers = Enumerable.Range(0, 4).Select(p => Task.Run(() =>
		{
			for (var i = 0; i < 100; i++) { source.OnNext(p * 100 + i); }
		})).ToArray();

		await Task.WhenAll(producers);
		await WaitFor(() => Volatile.Read(ref active) == 0 && !overlap, 50);

		Assert.IsFalse(overlap);
	}

	[TestMethod]
	public async Task ConflateLatestHandlesReentrantOnNextDuringDelivery()
	{
		var scheduler = new EventLoopScheduler();
		var source = new Subject<int>();
		var results = new List<int>();
		var reentered = false;
		using var sub = source.ConflateLatest(scheduler).Subscribe(val =>
		{
			results.Add(val);
			if (val == 1 && !reentered)
			{
				reentered = true;
				source.OnNext(2);
			}
		});

		source.OnNext(1);
		await WaitFor(() => results.Contains(2));

		CollectionAssert.AreEqual(new[] { 1, 2 }, results);
	}

	[TestMethod]
	public async Task ConflateLatestHandlesReentrantDisposeDuringDelivery()
	{
		var scheduler = new EventLoopScheduler();
		var source = new Subject<int>();
		var results = new List<int>();
		IDisposable sub = null;
		sub = source.ConflateLatest(scheduler).Subscribe(val =>
		{
			results.Add(val);
			sub.Dispose();
			source.OnNext(2);
		});

		source.OnNext(1);
		await Task.Delay(100);

		CollectionAssert.AreEqual(new[] { 1 }, results);
	}

	[TestMethod]
	public void ConflateLatestWithImmediateSchedulerDoesNotStackOverflow()
	{
		var results = new List<int>();
		using var sub = Observable.Range(1, 10000).ConflateLatest(ImmediateScheduler.Instance).Subscribe(results.Add);

		Assert.AreEqual(10000, results.Count);
	}

	[TestMethod]
	public void ConflateLatestWithCurrentThreadSchedulerDoesNotStackOverflow()
	{
		var results = new List<int>();
		using var sub = Observable.Range(1, 10000).ConflateLatest(CurrentThreadScheduler.Instance).Subscribe(results.Add);

		Assert.AreEqual(10000, results.Count);
	}

	[TestMethod]
	public async Task ConflateLatestCompletesWhenSourceCompletesWhileIdle()
	{
		var scheduler = new EventLoopScheduler();
		var source = new Subject<int>();
		var results = new List<int>();
		var completed = false;
		using var sub = source.ConflateLatest(scheduler).Subscribe(results.Add, () => completed = true);

		source.OnNext(1);
		await WaitFor(() => results.Count == 1);
		source.OnCompleted();
		await WaitFor(() => completed);

		Assert.IsTrue(completed);
		CollectionAssert.AreEqual(new[] { 1 }, results);
	}

	[TestMethod]
	public async Task ConflateLatestWithSyncContextForwardsSingleValue()
	{
		var context = new QueuedSynchronizationContext();
		var source = new Subject<int>();
		var results = new List<int>();
		using var sub = source.ConflateLatest(context).Subscribe(results.Add);

		source.OnNext(1);
		await WaitFor(() => results.Count == 1);

		CollectionAssert.AreEqual(new[] { 1 }, results);
		context.Dispose();
	}

	[TestMethod]
	public async Task ConflateLatestWithSyncContextCollapsesBurstToLatest()
	{
		var context = new QueuedSynchronizationContext();
		var source = new Subject<int>();
		var results = new List<int>();
		var firstDeliveryStarted = new ManualResetEventSlim(false);
		var releaseFirstDelivery = new ManualResetEventSlim(false);
		using var sub = source.ConflateLatest(context).Subscribe(val =>
		{
			lock (results) { results.Add(val); }
			if (val == 1)
			{
				firstDeliveryStarted.Set();
				releaseFirstDelivery.Wait();
			}
		});

		source.OnNext(1);
		Assert.IsTrue(firstDeliveryStarted.Wait(TimeSpan.FromSeconds(2)), "First delivery never started.");
		source.OnNext(2);
		source.OnNext(3);
		releaseFirstDelivery.Set();

		await WaitFor(() => { lock (results) { return results.Count == 2; } });
		lock (results) { CollectionAssert.AreEqual(new[] { 1, 3 }, results); }
		context.Dispose();
	}

	[TestMethod]
	public async Task ConflateLatestWithSyncContextDeliversOnContextThread()
	{
		var context = new QueuedSynchronizationContext();
		var source = new Subject<int>();
		var threads = new List<Thread>();
		using var sub = source.ConflateLatest(context).Subscribe(_ => threads.Add(Thread.CurrentThread));

		source.OnNext(1);
		await WaitFor(() => threads.Count == 1);
		source.OnNext(2);
		await WaitFor(() => threads.Count == 2);

		Assert.AreEqual(2, threads.Count);
		Assert.AreSame(threads[0], threads[1]);
		Assert.AreSame(context.ContextThread, threads[0]);
		context.Dispose();
	}

	[TestMethod]
	public async Task ConflateLatestWithSyncContextForwardsError()
	{
		var context = new QueuedSynchronizationContext();
		var source = new Subject<int>();
		var error = new TestException();
		Exception caught = null;
		using var sub = source.ConflateLatest(context).Subscribe(_ => { }, ex => caught = ex);

		source.OnNext(1);
		await Task.Delay(50);
		source.OnError(error);
		await WaitFor(() => caught != null);

		Assert.AreSame(error, caught);
		context.Dispose();
	}

	[TestMethod]
	public async Task ConflateLatestWithSyncContextCompletesAfterPendingValue()
	{
		var context = new QueuedSynchronizationContext();
		var source = new Subject<int>();
		var results = new List<int>();
		var completed = false;
		using var sub = source.ConflateLatest(context).Subscribe(results.Add, () => completed = true);

		source.OnNext(1);
		source.OnNext(2);
		source.OnCompleted();
		await WaitFor(() => completed);

		Assert.IsTrue(completed);
		context.Dispose();
	}

	[TestMethod]
	public async Task ConflateLatestWithSyncContextStopsEmittingAfterDispose()
	{
		var context = new QueuedSynchronizationContext();
		var source = new Subject<int>();
		var results = new List<int>();
		var sub = source.ConflateLatest(context).Subscribe(results.Add);

		source.OnNext(1);
		await WaitFor(() => results.Count == 1);
		sub.Dispose();
		source.OnNext(2);

		await Task.Delay(100);
		CollectionAssert.AreEqual(new[] { 1 }, results);
		context.Dispose();
	}

	[TestMethod]
	public async Task ConflateLatestWithSyncContextNeverDeliversConcurrently()
	{
		var context = new QueuedSynchronizationContext();
		var source = new Subject<int>();
		var active = 0;
		var overlap = false;
		using var sub = source.ConflateLatest(context).Subscribe(_ =>
		{
			if (Interlocked.Increment(ref active) > 1) { overlap = true; }
			Thread.Sleep(1);
			Interlocked.Decrement(ref active);
		});

		var producers = Enumerable.Range(0, 4).Select(p => Task.Run(() =>
		{
			for (var i = 0; i < 100; i++) { source.OnNext(p * 100 + i); }
		})).ToArray();

		await Task.WhenAll(producers);
		await WaitFor(() => Volatile.Read(ref active) == 0 && !overlap, 50);

		Assert.IsFalse(overlap);
		context.Dispose();
	}

	private sealed class QueuedSynchronizationContext : SynchronizationContext, IDisposable
	{
		private readonly BlockingCollection<(SendOrPostCallback Callback, object State)> queue = new();

		public QueuedSynchronizationContext()
		{
			ContextThread = new Thread(() =>
			{
				foreach (var (callback, state) in queue.GetConsumingEnumerable())
				{
					callback(state);
				}
			})
			{
				IsBackground = true,
				Name = "queued-sync-context"
			};
			ContextThread.Start();
		}

		public Thread ContextThread { get; }

		public override void Post(SendOrPostCallback d, object state) => queue.Add((d, state));

		public void Dispose()
		{
			queue.CompleteAdding();
			ContextThread.Join(TimeSpan.FromSeconds(2));
			queue.Dispose();
		}
	}

	private static async Task WaitFor(Func<bool> condition, int timeoutMs = 2000)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
		while (!condition())
		{
			if (DateTime.UtcNow > deadline) { Assert.Fail("Condition not met within timeout."); }
			await Task.Delay(10);
		}
	}
}
