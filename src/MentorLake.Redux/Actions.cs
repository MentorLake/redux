using System.Reactive.Linq;

namespace MentorLake.Redux;

/// <summary>
/// Marker action dispatched while the store initializes reducers. Rarely used directly.
/// </summary>
public class InitializeStoreAction
{
}

/// <summary>
/// An action with a stable string name, enabling name-based matching in reducers and
/// effects. Use for thunk lifecycle actions and any action whose identity should survive
/// refactoring of CLR types.
/// </summary>
public interface IAction
{
	/// <summary>The stable name this action is matched by.</summary>
	public string ActionName { get; }
}

public static class ActionExtensions
{
	/// <summary>
	/// Filters the action stream to <see cref="IAction"/>s with the given name.
	/// </summary>
	/// <param name="source">Usually <c>store.Actions</c>.</param>
	/// <param name="actionName">The exact <see cref="IAction.ActionName"/> to match.</param>
	public static IObservable<IAction> Action(this IObservable<object> source, string actionName)
	{
		return source
			.Where(a => a is IAction action && action.ActionName == actionName)
			.Select(a => a as IAction);
	}

	/// <summary>
	/// Filters the action stream to actions of type <typeparamref name="T"/> that also
	/// carry the given name.
	/// </summary>
	/// <typeparam name="T">The CLR action type.</typeparam>
	/// <param name="source">Usually <c>store.Actions</c>.</param>
	/// <param name="actionName">The exact <see cref="IAction.ActionName"/> to match.</param>
	public static IObservable<T> Action<T>(this IObservable<object> source, string actionName) where T : IAction
	{
		return source
			.Where(a => a is T action && action.ActionName == actionName)
			.Select(a => (T)a);
	}
}
