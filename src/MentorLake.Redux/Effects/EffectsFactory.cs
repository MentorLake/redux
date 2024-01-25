namespace MentorLake.Redux.Effects;

public static class EffectsFactory
{
	public static Effect Create(Func<IObservable<object>, IObservable<object>> effectObs, EffectConfig config = null)
	{
		return new Effect
		{
			Run = effectObs,
			Config = config ?? new EffectConfig() { Dispatch = false }
		};
	}
}
