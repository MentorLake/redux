namespace MentorLake.Redux.Middleware;

/// <summary>
/// Creates middleware from delegates, avoiding a class per concern.
/// </summary>
public static class ReduxMiddleware
{
	/// <summary>
	/// Builds middleware from a handler delegate.
	/// </summary>
	/// <param name="handler">Receives each action and the <c>next</c> continuation.</param>
	/// <param name="initialize">Optional one-time setup when the middleware is installed.</param>
	public static IReduxMiddleware Create(Action<object, NextMiddleware> handler, Action<ReduxStore> initialize = null)
	{
		ArgumentNullException.ThrowIfNull(handler);
		return new FuncMiddleware(handler, initialize);
	}

	private sealed class FuncMiddleware(Action<object, NextMiddleware> handler, Action<ReduxStore> initialize) : IReduxMiddleware
	{
		public void Initialize(ReduxStore store) => initialize?.Invoke(store);

		public void Invoke(object action, NextMiddleware next) => handler(action, next);
	}
}
