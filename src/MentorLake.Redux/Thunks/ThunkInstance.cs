using System.Reactive;

namespace MentorLake.Redux.Thunks;

public class ThunkInstance<T> : ThunkInstance<T, Unit>
{
	public ThunkInstance(Func<ThunkApi, Task> work)
		: base(async api => { await work(api); return Unit.Default; })
	{

	}
}

public class ThunkInstance<T, TResult>(Func<ThunkApi, Task<TResult>> work)
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending<T>());
			var result = await work(api);
			api.Dispatch(new ThunkFulfilled<T, TResult>(result));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected<T>(e));
		}
	}
}
