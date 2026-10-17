using System.Reactive.Subjects;

namespace MentorLake.Redux.Thunks;

public sealed class ThunkApiContext
{
	internal ThunkApiContext(Subject<object> actionDispatcher, ReduxStore store, CancellationToken cancellationToken = default)
	{
		ActionDispatcher = actionDispatcher;
		Store = store;
		CancellationToken = cancellationToken;
	}

	internal Subject<object> ActionDispatcher { get; }

	public ReduxStore Store { get; }

	public CancellationToken CancellationToken { get; }
}
