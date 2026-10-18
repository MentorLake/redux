using System.Reactive.Linq;

namespace MentorLake.Redux.Effects;

/// <summary>
/// Creates effects from a raw Rx transform over the action stream. For trigger/handler
/// style effects, prefer <see cref="EffectBuilder"/> or <see cref="Effect.Build"/>.
/// </summary>
public static class EffectsFactory
{
	/// <summary>
	/// Wraps an Rx pipeline as an effect.
	/// </summary>
	/// <typeparam name="T">The effect's output type.</typeparam>
	/// <param name="effectObs">Transform from the action stream to the output stream.</param>
	/// <param name="config">Effect options; defaults to observe-only (no dispatch).</param>
	public static Effect Create<T>(Func<IObservable<object>, IObservable<T>> effectObs, EffectConfig config = null)
	{
		return new Effect
		{
			Run = actions => effectObs(actions).Select(x => (object) x),
			Config = config ?? new EffectConfig() { Dispatch = false }
		};
	}
}
