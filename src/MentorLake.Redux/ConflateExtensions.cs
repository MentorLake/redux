using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace MentorLake.Redux;

/// <summary>
/// Rx operators for coalescing bursts of store updates.
/// </summary>
public static class ConflateExtensions
{
	/// <summary>
	/// Delivers values on <paramref name="scheduler"/> with conflation: at most one
	/// emission is in flight, and while it runs, new source values overwrite a single
	/// pending slot — the newest value always wins and intermediate values are dropped.
	/// Unlike <c>Switch</c>, in-flight work is never canceled; unlike <c>Sample</c>, no
	/// clock is needed — the consumer's own throughput is the cadence.
	/// </summary>
	/// <typeparam name="T">The value type.</typeparam>
	/// <param name="source">The source stream (e.g. a selector subscription).</param>
	/// <param name="scheduler">Where deliveries run. <c>ThreadPoolScheduler</c> collapses
	/// bursts onto the pool; an <c>EventLoopScheduler</c> adds thread affinity.</param>
	/// <remarks>
	/// Errors and completion pass through immediately; disposal stops pending emissions.
	/// See docs/slow-consumers.md.
	/// </remarks>
	public static IObservable<T> ConflateLatest<T>(this IObservable<T> source, IScheduler scheduler)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(scheduler);

		return source.ConflateLatest(work => scheduler.Schedule(work));
	}

	/// <summary>
	/// Delivers values on <paramref name="synchronizationContext"/> with conflation: at most one
	/// emission is in flight, and while it runs, new source values overwrite a single
	/// pending slot — the newest value always wins and intermediate values are dropped.
	/// Unlike <c>Switch</c>, in-flight work is never canceled; unlike <c>Sample</c>, no
	/// clock is needed — the consumer's own throughput is the cadence.
	/// </summary>
	/// <typeparam name="T">The value type.</typeparam>
	/// <param name="source">The source stream (e.g. a selector subscription).</param>
	/// <param name="synchronizationContext">Where deliveries run, e.g. a Blazor
	/// <c>DispatcherSynchronizationContext</c>. Deliveries are posted to the context, so
	/// the source thread is never blocked and the context's own throughput is the cadence.</param>
	/// <remarks>
	/// Errors and completion pass through immediately; disposal stops pending emissions.
	/// See docs/slow-consumers.md.
	/// </remarks>
	public static IObservable<T> ConflateLatest<T>(this IObservable<T> source, SynchronizationContext synchronizationContext)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(synchronizationContext);

		return source.ConflateLatest(work => synchronizationContext.Post(_ => work(), null));
	}

	private static IObservable<T> ConflateLatest<T>(this IObservable<T> source, Action<Action> dispatch)
	{
		return Observable.Create<T>(observer =>
		{
			var gate = new object();
			var hasValue = false;
			var inFlight = false;
			var disposed = false;
			var latest = default(T)!;
			var sourceSubscription = new SingleAssignmentDisposable();

			void Drain()
			{
				T next;
				lock (gate)
				{
					if (!hasValue || inFlight || disposed) { return; }
					inFlight = true;
					hasValue = false;
					next = latest;
					latest = default!;
				}

				dispatch(() =>
				{
					try
					{
						observer.OnNext(next);
					}
					finally
					{
						lock (gate) { inFlight = false; }
						Drain();
					}
				});
			}

			sourceSubscription.Disposable = source.Subscribe(
				val => { lock (gate) { hasValue = true; latest = val; } Drain(); },
				observer.OnError,
				() => { lock (gate) { disposed = true; } Drain(); observer.OnCompleted(); });

			return Disposable.Create(() =>
			{
				lock (gate) { disposed = true; }
				sourceSubscription.Dispose();
			});
		});
	}
}
