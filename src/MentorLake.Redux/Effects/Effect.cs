namespace MentorLake.Redux.Effects;

/// <summary>
/// A side-effect subscription over the action stream. Build with <see cref="Build"/> or
/// <see cref="EffectsFactory.Create{T}"/> and register with <c>store.RegisterEffects</c>.
/// </summary>
public class Effect
{
	/// <summary>Transforms the action stream into the effect's output stream.</summary>
	public Func<IObservable<object>, IObservable<object>> Run { get; init; }

	/// <summary>Controls whether outputs are re-dispatched into the store.</summary>
	public EffectConfig Config { get; init; }

	/// <summary>Starts the fluent builder: <c>Effect.Build().On&lt;TAction&gt;().Handle(...)</c>.</summary>
	public static EffectBuilder Build() => EffectBuilder.Create();
}
