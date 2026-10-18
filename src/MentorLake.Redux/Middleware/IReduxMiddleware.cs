namespace MentorLake.Redux.Middleware;

/// <summary>
/// Passes an action to the next stage of the pipeline (ultimately reduce-and-notify).
/// Calling it runs the rest of the pipeline synchronously on the processor thread; not
/// calling it swallows the action.
/// </summary>
public delegate void NextMiddleware(object action);

/// <summary>
/// Intercepts every dispatched action before it reaches the reducers. Use for logging,
/// telemetry, action rewriting, or conditional suppression. Must be synchronous and fast —
/// it runs on the store's processor thread. Create ad-hoc middleware with
/// <see cref="ReduxMiddleware.Create"/>.
/// </summary>
public interface IReduxMiddleware
{
	/// <summary>Called once when the middleware is installed via <c>store.UseMiddleware</c>.</summary>
	void Initialize(ReduxStore store)
	{
	}

	/// <summary>
	/// Handles an action. Do the work, call <paramref name="next"/> to continue the
	/// pipeline (or don't, to swallow), and keep everything synchronous.
	/// </summary>
	/// <param name="action">The dispatched action.</param>
	/// <param name="next">Continues the pipeline with this action.</param>
	void Invoke(object action, NextMiddleware next);
}
