using System.Reactive.Subjects;

namespace MentorLake.Redux.Thunks;

/// <summary>
/// The API surface handed to thunk work: live state, dispatch, cancellation, and nested
/// thunk dispatch. Subclass to add application services (constructor-injected via a
/// factory registered with <c>store.RegisterThunkApiFactory</c>) — the recommended pattern
/// for keeping services out of components.
/// </summary>
public partial class ThunkApi(Subject<object> actionDispatcher, ReduxStore store, CancellationToken cancellationToken = default)
{
	/// <summary>Creates the API from its run context. Custom API subtypes take this in their constructor.</summary>
	public ThunkApi(ThunkApiContext context)
		: this(context.ActionDispatcher, context.Store, context.CancellationToken)
	{
	}

	/// <summary>
	/// The store's state at read time — always live, including actions dispatched earlier
	/// within the same thunk run.
	/// </summary>
	public StoreState State => store.State;

	/// <summary>
	/// Cancellation for this run: honors the caller's token and group cancellation from
	/// <c>DispatchThunkLatest</c>. Pass it to async work.
	/// </summary>
	public CancellationToken CancellationToken => cancellationToken;

	/// <summary>
	/// Dispatches an action through the store pipeline.
	/// </summary>
	/// <param name="action">The action to dispatch.</param>
	public void Dispatch(object action)
	{
		actionDispatcher.OnNext(action);
	}

	/// <summary>
	/// Dispatches nested thunks against a different (typically the custom) API type.
	/// </summary>
	/// <typeparam name="TApi">The API type the nested thunks expect.</typeparam>
	public ThunkDispatcher<TApi> Using<TApi>() where TApi : ThunkApi
	{
		return store.Using<TApi>();
	}
}
