using System.Collections.Immutable;

namespace MentorLake.Redux;

/// <summary>
/// Immutable container mapping feature state types to their current values. The store
/// produces a new instance per state change; equality is reference-based.
/// </summary>
public class StoreState
{
	private ImmutableDictionary<Type, object> _featureDictionary = ImmutableDictionary<Type, object>.Empty;

	/// <summary>
	/// Reads the slice registered for <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The feature state type (each feature owns one exclusively).</typeparam>
	/// <exception cref="KeyNotFoundException">No reducer registered this feature.</exception>
	public T GetFeatureState<T>() where T : class
	{
		return _featureDictionary[typeof(T)] as T;
	}

	/// <summary>
	/// Returns a new <see cref="StoreState"/> with <typeparamref name="T"/>'s slice replaced.
	/// Used by reducers; never mutates this instance.
	/// </summary>
	public StoreState UpdateFeatureState<T>(T state) where T : class
	{
		return new StoreState() { _featureDictionary = _featureDictionary.SetItem(typeof(T), state) };
	}

	public bool Equals(StoreState other)
	{
		return ReferenceEquals(this, other);
	}
}
