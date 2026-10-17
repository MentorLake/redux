using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace MentorLake.Redux.Effects;

public enum EffectStrategy
{
	Merge,

	Switch,

	Exhaust
}

public sealed class EffectBuilder
{
	private Func<object, bool> _trigger = static _ => true;
	private Func<object, IObservable<object>> _handler;
	private EffectStrategy _strategy = EffectStrategy.Merge;
	private TimeSpan? _debounce;
	private TimeSpan? _throttle;
	private bool _dispatchResults;

	public static EffectBuilder Create() => new();

	public EffectBuilder On<TAction>() => On(static action => action is TAction);

	public EffectBuilder On(Func<object, bool> predicate)
	{
		ArgumentNullException.ThrowIfNull(predicate);
		_trigger = predicate;
		return this;
	}

	public EffectBuilder Handle(Func<object, IObservable<object>> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);
		_handler = handler;
		return this;
	}

	public EffectBuilder TakeLatest() => WithStrategy(EffectStrategy.Switch);

	public EffectBuilder TakeEvery() => WithStrategy(EffectStrategy.Merge);

	public EffectBuilder Exhaust() => WithStrategy(EffectStrategy.Exhaust);

	public EffectBuilder Debounce(TimeSpan dueTime)
	{
		_debounce = dueTime;
		return this;
	}

	public EffectBuilder Throttle(TimeSpan window)
	{
		_throttle = window;
		return this;
	}

	public EffectBuilder DispatchResults()
	{
		_dispatchResults = true;
		return this;
	}

	private EffectBuilder WithStrategy(EffectStrategy strategy)
	{
		_strategy = strategy;
		return this;
	}

	public Effect Build()
	{
		if (_handler == null)
		{
			throw new InvalidOperationException("An effect requires a handler. Call Handle(...) before Build().");
		}

		var trigger = _trigger;
		var handler = _handler;
		var debounce = _debounce;
		var throttle = _throttle;
		var strategy = _strategy;

		return new Effect
		{
			Run = actions =>
			{
				var source = actions.Where(trigger);
				if (debounce.HasValue) source = Debounced(source, debounce.Value);
				if (throttle.HasValue) source = source.Throttle(throttle.Value);

				var inner = source.Select(handler);
				return strategy switch
				{
					EffectStrategy.Switch => inner.Switch(),
					EffectStrategy.Exhaust => Exhaust(inner),
					_ => inner.Merge()
				};
			},
			Config = new EffectConfig { Dispatch = _dispatchResults }
		};
	}

	private static IObservable<object> Debounced(IObservable<object> source, TimeSpan dueTime)
	{
		return source.Select(action => Observable.Timer(dueTime).Select(_ => action)).Switch();
	}

	private static IObservable<object> Exhaust(IObservable<IObservable<object>> sources)
	{
		return Observable.Create<object>(observer =>
		{
			var gate = new object();
			var busy = false;
			var outerDone = false;
			var innerSubscription = new SerialDisposable();

			var outerSubscription = sources.Subscribe(
				inner =>
				{
					lock (gate)
					{
						if (busy) return;
						busy = true;
					}

					innerSubscription.Disposable = inner.Subscribe(
						observer.OnNext,
						observer.OnError,
						() =>
						{
							bool completeNow;
							lock (gate)
							{
								busy = false;
								completeNow = outerDone;
							}

							if (completeNow) observer.OnCompleted();
						});
				},
				observer.OnError,
				() =>
				{
					bool completeNow;
					lock (gate)
					{
						outerDone = true;
						completeNow = !busy;
					}

					if (completeNow) observer.OnCompleted();
				});

			return new CompositeDisposable(outerSubscription, innerSubscription);
		});
	}
}
