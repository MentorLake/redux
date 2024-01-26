using System.Reactive.Linq;

namespace MentorLake.Redux.Effects;

public static class EffectsFactory
{
	public static Effect Create<T>(Func<IObservable<object>, IObservable<T>> effectObs, EffectConfig config = null)
	{
		return new Effect
		{
			Run = actions => effectObs(actions).Select(x => (object) x),
			Config = config ?? new EffectConfig() { Dispatch = false }
		};
	}
}
