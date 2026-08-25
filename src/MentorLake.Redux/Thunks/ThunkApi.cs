using System.Reactive.Subjects;

namespace MentorLake.Redux.Thunks;

public partial class ThunkApi(Subject<object> actionDispatcher, ReduxStore store)
{
	public ThunkApi(ThunkApiContext context)
		: this(context.ActionDispatcher, context.Store)
	{
	}

	public StoreState State => store.State;

	public void Dispatch(object action)
	{
		actionDispatcher.OnNext(action);
	}

	public ThunkDispatcher<TApi> Using<TApi>() where TApi : ThunkApi
	{
		return store.Using<TApi>();
	}
}
