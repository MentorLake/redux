using System.Reactive.Subjects;

namespace MentorLake.Redux.Thunks;

public class ThunkApi(Subject<object> actionDispatcher)
{
	public StoreState State { get; set; }

	public void Dispatch(object action)
	{
		actionDispatcher.OnNext(action);
	}
}
