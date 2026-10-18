namespace MentorLake.Redux.Selectors;

/// <summary>
/// A named, composable projection of store state. Apply to a snapshot for a one-shot read,
/// or to the state stream for change-filtered observation. Build with
/// <see cref="SelectorFactory"/>; compose selectors from other selectors for memoized,
/// shared evaluation.
/// </summary>
/// <typeparam name="TOutput">The projected value.</typeparam>
public interface ISelector<out TOutput>
{
	/// <summary>Projects a single state snapshot.</summary>
	TOutput Apply(StoreState input);

	/// <summary>
	/// Projects a state stream, emitting the current value on subscription and again only
	/// when the output changes (per the selector's comparer).
	/// </summary>
	IObservable<TOutput> Apply(IObservable<StoreState> input);
}
