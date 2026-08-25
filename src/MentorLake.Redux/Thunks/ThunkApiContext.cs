using System.Reactive.Subjects;

namespace MentorLake.Redux.Thunks;

public sealed class ThunkApiContext
{
	internal ThunkApiContext(Subject<object> actionDispatcher, ReduxStore store)
	{
		ActionDispatcher = actionDispatcher;
		Store = store;
	}

	internal Subject<object> ActionDispatcher { get; }

	public ReduxStore Store { get; }
}
