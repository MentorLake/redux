using System.Reactive.Subjects;

namespace MentorLake.Redux.Thunks;

public partial class ThunkApi(Subject<object> actionDispatcher, ReduxStore store)
{
	public StoreState State => store.State;

	public void Dispatch(object action)
	{
		actionDispatcher.OnNext(action);
	}

	public ThunkResult DispatchThunk(ICallableThunkAction thunk)
	{
		return store.DispatchThunk(thunk);
	}

	public ThunkResult<TResult> DispatchThunk<TResult>(ICallableThunkFunc<TResult> thunk)
	{
		return store.DispatchThunk(thunk);
	}

	public ThunkResult DispatchThunk(Func<ThunkApi, Task> work)
	{
		return store.DispatchThunk(work);
	}

	public ThunkResult<TResult> DispatchThunk<TResult>(Func<ThunkApi, Task<TResult>> work)
	{
		return store.DispatchThunk(work);
	}
}
