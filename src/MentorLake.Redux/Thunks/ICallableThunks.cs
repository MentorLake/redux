namespace MentorLake.Redux.Thunks;

public interface ICallableThunkAction
{
	public Task ExecuteAsync(ThunkApi api);
}

public interface ICallableThunkFunc<TResult> : ICallableThunkAction
{

}
