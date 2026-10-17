namespace MentorLake.Redux.Middleware;

public static class ReduxMiddleware
{
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
