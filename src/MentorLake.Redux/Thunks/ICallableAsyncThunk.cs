namespace MentorLake.Redux.Thunks;

public interface ICallableAsyncThunk
{
	public Task ExecuteAsync(ThunkApi api);
}

public interface ICallableAsyncThunk<TResult> : ICallableAsyncThunk
{

}
