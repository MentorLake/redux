namespace MentorLake.Redux.Effects;

/// <summary>
/// Groups an application's effects for registration. Implement, register in DI, and pass
/// to <c>store.RegisterEffects</c> (or discover via <c>store.RegisterAssembly</c>).
/// </summary>
public interface IEffectsFactory
{
	/// <summary>Creates the effects this factory contributes. Called once at registration.</summary>
	IEnumerable<Effect> Create();
}
