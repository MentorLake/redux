namespace MentorLake.Redux.Effects;

public interface IEffectsFactory
{
	IEnumerable<Effect> Create();
}
