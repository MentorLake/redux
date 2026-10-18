using System.Reactive.Subjects;

namespace MentorLake.Redux.Thunks;

/// <summary>
/// Everything needed to construct a <see cref="ThunkApi"/> for one run. Custom API
/// subtypes accept this in their constructor and are created by factories registered with
/// <c>store.RegisterThunkApiFactory</c>.
/// </summary>
public sealed class ThunkApiContext
{
	internal ThunkApiContext(Subject<object> actionDispatcher, ReduxStore store, CancellationToken cancellationToken = default)
	{
		ActionDispatcher = actionDispatcher;
		Store = store;
		CancellationToken = cancellationToken;
	}

	internal Subject<object> ActionDispatcher { get; }

	/// <summary>The store this thunk runs against.</summary>
	public ReduxStore Store { get; }

	/// <summary>Cancellation for this thunk run (caller token combined with group cancellation).</summary>
	public CancellationToken CancellationToken { get; }
}
