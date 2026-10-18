using System.Collections.Immutable;

namespace MentorLake.Redux.Selectors;

/// <summary>
/// Composable refinements for selectors: attach a custom change-test without rebuilding
/// the projection.
/// </summary>
public static class SelectorExtensions
{
	/// <summary>
	/// Returns a selector that emits only when the comparison says the value changed.
	/// Use when default equality is too strict (e.g. compare only one field of a record).
	/// </summary>
	/// <typeparam name="TResult">The selector's output type.</typeparam>
	/// <param name="selector">The selector to refine.</param>
	/// <param name="areEqual">Change test applied to consecutive outputs.</param>
	public static ISelector<TResult> WithComparer<TResult>(this ISelector<TResult> selector, Func<TResult, TResult, bool> areEqual)
	{
		return SelectorFactory.Create(selector, s => s, areEqual);
	}

	/// <summary>
	/// For list-valued selectors: emits only when the sequence changes element-by-element
	/// under <paramref name="f"/>. Valuable when the list is rebuilt on every recomputation
	/// (so reference equality always fails) but only semantic changes matter.
	/// </summary>
	/// <typeparam name="T">The element type.</typeparam>
	/// <param name="selector">The list selector to refine.</param>
	/// <param name="f">Element comparison applied pairwise.</param>
	public static ISelector<ImmutableList<T>> WithSequenceComparer<T>(this ISelector<ImmutableList<T>> selector, Func<T, T, bool> f)
	{
		return SelectorFactory.Create(selector, s => s, FuncEqualityComparer<ImmutableList<T>>.Create((x, y) =>
		{
			return x.SequenceEqual(y, FuncEqualityComparer<T>.Create(f));
		}));
	}
}
