namespace MentorLake.Redux.Thunks;

/// <summary>
/// Dispatch entry point scoped to a <see cref="ThunkApi"/> type, obtained from
/// <c>store.Using&lt;TApi&gt;()</c>. The <c>DispatchThunk</c>/<c>DispatchThunkLatest</c>
/// overloads (generated from DispatchThunkOverloads.tt) accept thunks, bound thunks, and
/// bare lambdas with arguments.
/// </summary>
/// <typeparam name="TApi">The API type the dispatched thunks run against.</typeparam>
public sealed partial class ThunkDispatcher<TApi> where TApi : ThunkApi
{
	private readonly ReduxStore _store;

	internal ThunkDispatcher(ReduxStore store)
	{
		_store = store;
	}
}
