namespace MentorLake.Redux.Reducers;

/// <summary>
/// A reducer that owns one feature slice: registers its initial state and contributes
/// action handlers. Implemented by <see cref="FeatureReducer{TFeatureState}"/>; consume
/// the interface when writing custom registration logic.
/// </summary>
public interface IFeatureReducer
{
	/// <summary>The action handlers this feature contributes, lifted to operate on the whole <see cref="StoreState"/>.</summary>
	IEnumerable<ActionReducer<StoreState>> ActionReducers { get; }

	/// <summary>Seeds this feature's initial value into the store state.</summary>
	StoreState InitializeStore(StoreState state);
}
