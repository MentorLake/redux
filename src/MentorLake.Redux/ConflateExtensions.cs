using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace MentorLake.Redux;

public static class ConflateExtensions
{
	public static IObservable<T> ConflateLatest<T>(this IObservable<T> source, IScheduler scheduler)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(scheduler);

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

				scheduler.Schedule(() =>
				{
					observer.OnNext(next);
					lock (gate) { inFlight = false; }
					Drain();
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
