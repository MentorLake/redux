using System.Reactive.Linq;

namespace MentorLake.Redux;

public class InitializeStoreAction
{
}

public interface IAction
{
	public string ActionName { get; }
}

public static class ActionExtensions
{
	public static IObservable<IAction> Action(this IObservable<object> source, string actionName)
	{
		return source
			.Where(a => a is IAction action && action.ActionName == actionName)
			.Select(a => a as IAction);
	}

	public static IObservable<T> Action<T>(this IObservable<object> source, string actionName) where T : IAction
	{
		return source
			.Where(a => a is T action && action.ActionName == actionName)
			.Select(a => (T)a);
	}
}
