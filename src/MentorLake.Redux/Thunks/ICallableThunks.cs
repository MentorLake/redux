namespace MentorLake.Redux.Thunks;

/// <summary>
/// A thunk action bound for execution, produced by <see cref="ThunkAction{TApi}.Bind"/>.
/// Matches the parameterless overloads of <c>DispatchThunk</c>/<c>DispatchThunkLatest</c>.
/// </summary>
/// <typeparam name="TApi">The API type the thunk runs against.</typeparam>
public interface ICallableThunkAction<TApi> where TApi : ThunkApi
{
	/// <summary>
	/// Runs the work, dispatching pending/fulfilled/rejected/cancelled lifecycle actions
	/// around it. Exceptions are captured into <see cref="ThunkRejected"/>, not thrown.
	/// </summary>
	/// <param name="api">The thunk API providing state, dispatch, and cancellation.</param>
	Task ExecuteAsync(TApi api);
}

/// <summary>
/// A thunk function (one that returns a value) bound for execution, produced by
/// <see cref="ThunkFunc{TApi, TResult}.Bind"/>.
/// </summary>
/// <typeparam name="TApi">The API type the thunk runs against.</typeparam>
/// <typeparam name="TResult">The thunk's return type.</typeparam>
public interface ICallableThunkFunc<TApi, TResult> : ICallableThunkAction<TApi> where TApi : ThunkApi
{

}
