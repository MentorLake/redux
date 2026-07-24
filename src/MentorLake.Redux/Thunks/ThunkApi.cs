using System.Reactive.Subjects;

namespace MentorLake.Redux.Thunks;

public class ThunkApi(Subject<object> actionDispatcher, ReduxStore store)
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
}
