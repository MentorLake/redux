namespace MentorLake.Redux.Middleware;

public delegate void NextMiddleware(object action);

public interface IReduxMiddleware
{
	void Initialize(ReduxStore store)
	{
	}

	void Invoke(object action, NextMiddleware next);
}
