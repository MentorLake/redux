namespace MentorLake.Redux.Effects;

public class Effect
{
	public Func<IObservable<object>, IObservable<object>> Run { get; init; }
	public EffectConfig Config { get; init; }
}
