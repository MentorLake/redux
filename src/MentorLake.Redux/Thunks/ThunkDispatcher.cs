namespace MentorLake.Redux.Thunks;

public sealed partial class ThunkDispatcher<TApi> where TApi : ThunkApi
{
	private readonly ReduxStore _store;

	internal ThunkDispatcher(ReduxStore store)
	{
		_store = store;
	}
}
