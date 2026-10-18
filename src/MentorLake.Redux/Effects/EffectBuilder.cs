using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace MentorLake.Redux.Effects;

/// <summary>
/// How an effect handles overlapping handler runs triggered by new matching actions.
/// </summary>
public enum EffectStrategy
{
	/// <summary>Run every trigger; overlapping handlers execute concurrently.</summary>
	Merge,

	/// <summary>Each new trigger cancels the previous handler run (search-as-you-type).</summary>
	Switch,

	/// <summary>Ignore new triggers while a handler run is in flight.</summary>
	Exhaust
}

/// <summary>
/// Fluent builder for effects: pick a trigger, a handler, an overlap strategy, and
/// optional timing filters. Example:
/// <c>Effect.Build().On&lt;SearchAction&gt;().TakeLatest().Handle(a =&gt; Search(a)).DispatchResults().Build()</c>.
/// </summary>
public sealed class EffectBuilder
{
	private Func<object, bool> _trigger = static _ => true;
	private Func<object, IObservable<object>> _handler;
	private EffectStrategy _strategy = EffectStrategy.Merge;
	private TimeSpan? _debounce;
	private TimeSpan? _throttle;
	private bool _dispatchResults;

	/// <summary>Creates a builder that triggers on every action until narrowed by <c>On</c>.</summary>
	public static EffectBuilder Create() => new();

	/// <summary>
	/// Triggers on actions of CLR type <typeparamref name="TAction"/>.
	/// </summary>
	public EffectBuilder On<TAction>() => On(static action => action is TAction);

	/// <summary>
	/// Triggers on actions matching an arbitrary predicate.
	/// </summary>
	/// <param name="predicate">Return true to run the handler for this action.</param>
	public EffectBuilder On(Func<object, bool> predicate)
	{
		ArgumentNullException.ThrowIfNull(predicate);
		_trigger = predicate;
		return this;
	}

	/// <summary>
	/// The work to run per trigger. Return an observable (e.g. from
	/// <c>Observable.FromAsync</c>) whose outputs are the effect's results.
	/// </summary>
	/// <param name="handler">Transform from the triggering action to a result stream.</param>
	public EffectBuilder Handle(Func<object, IObservable<object>> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);
		_handler = handler;
		return this;
	}

	/// <summary>New triggers cancel the in-flight handler run (see <see cref="EffectStrategy.Switch"/>).</summary>
	public EffectBuilder TakeLatest() => WithStrategy(EffectStrategy.Switch);

	/// <summary>Every trigger runs, overlapping allowed (see <see cref="EffectStrategy.Merge"/>).</summary>
	public EffectBuilder TakeEvery() => WithStrategy(EffectStrategy.Merge);

	/// <summary>New triggers are ignored while a handler run is in flight (see <see cref="EffectStrategy.Exhaust"/>).</summary>
	public EffectBuilder Exhaust() => WithStrategy(EffectStrategy.Exhaust);

	/// <summary>
	/// Waits for a quiet period: the handler runs only after no matching action arrives
	/// for <paramref name="dueTime"/>.
	/// </summary>
	public EffectBuilder Debounce(TimeSpan dueTime)
	{
		_debounce = dueTime;
		return this;
	}

	/// <summary>
	/// Rate-limits triggers: the first passes immediately, then at most one per
	/// <paramref name="window"/>.
	/// </summary>
	public EffectBuilder Throttle(TimeSpan window)
	{
		_throttle = window;
		return this;
	}

	/// <summary>
	/// Re-dispatches the effect's outputs as actions through the full pipeline. Without
	/// this, outputs are observed only and dropped.
	/// </summary>
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

	/// <summary>
	/// Produces the effect. Register it with <c>store.RegisterEffects</c>.
	/// </summary>
	/// <exception cref="InvalidOperationException">No handler was set via <c>Handle</c>.</exception>
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
