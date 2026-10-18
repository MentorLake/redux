namespace MentorLake.Redux.Reducers;

/// <summary>
/// A factory for feature reducers. Implement to group related feature reducers and
/// register them with <c>store.RegisterReducers</c> or <c>store.RegisterAssembly</c>.
/// </summary>
public interface IReducerFactory
{
	/// <summary>Creates the feature reducers this factory contributes. Called once at registration.</summary>
	FeatureReducerCollection Create();
}
