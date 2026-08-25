namespace MentorLake.Redux.Thunks;

public interface ICallableThunkAction<TApi> where TApi : ThunkApi
{
	Task ExecuteAsync(TApi api);
}

public interface ICallableThunkFunc<TApi, TResult> : ICallableThunkAction<TApi> where TApi : ThunkApi
{

}
