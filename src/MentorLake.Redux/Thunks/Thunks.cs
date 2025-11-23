
namespace MentorLake.Redux.Thunks;

public readonly struct AsyncThunk(string actionName, Func<ThunkApi, Task> work)
{
	private readonly CallableAsyncThunk _thunk = new(actionName, work);
	public CallableAsyncThunk Bind() => _thunk;
	public static implicit operator CallableAsyncThunk(AsyncThunk thunk) => thunk.Bind();
}

public readonly struct AsyncThunkReturnOnly<TResult>(string actionName, Func<ThunkApi, Task<TResult>> work)
{
	private readonly CallableAsyncThunkReturnOnly<TResult> _thunk = new(actionName, work);
	public CallableAsyncThunkReturnOnly<TResult> Bind() => _thunk;
	public static implicit operator CallableAsyncThunkReturnOnly<TResult>(AsyncThunkReturnOnly<TResult> thunk) => thunk.Bind();
}

public readonly struct CallableAsyncThunk(string actionName, Func<ThunkApi, Task> work) : ICallableAsyncThunk
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			await work(api);
			api.Dispatch(new ThunkFulfilled(actionName));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}

public readonly struct CallableAsyncThunkReturnOnly<TResult>(string actionName, Func<ThunkApi, Task<TResult>> work) : ICallableAsyncThunk<TResult>
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			var result = await work(api);
			api.Dispatch(new ThunkFulfilled<TResult>(actionName, result));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}

public readonly struct AsyncThunkArgOnly<TArg1>(string actionName, Func<ThunkApi, TArg1, Task> work)
{
	public CallableAsyncThunkArgOnly<TArg1> Bind(TArg1 arg1) => new(actionName, work, arg1);
}

public readonly struct AsyncThunk<TArg1, TResult>(string actionName, Func<ThunkApi, TArg1, Task<TResult>> work)
{
	public CallableAsyncThunk<TArg1, TResult> Bind(TArg1 arg1) => new(actionName, work, arg1);
}

public readonly struct CallableAsyncThunkArgOnly<TArg1>(string actionName, Func<ThunkApi, TArg1, Task> work, TArg1 arg1) : ICallableAsyncThunk
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			await work(api, arg1);
			api.Dispatch(new ThunkFulfilled(actionName));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}

public readonly struct CallableAsyncThunk<TArg1, TResult>(string actionName, Func<ThunkApi, TArg1, Task<TResult>> work, TArg1 arg1) : ICallableAsyncThunk<TResult>
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			var result = await work(api, arg1);
			api.Dispatch(new ThunkFulfilled<TResult>(actionName, result));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}


public readonly struct AsyncThunkArgOnly<TArg1,TArg2>(string actionName, Func<ThunkApi, TArg1,TArg2, Task> work)
{
	public CallableAsyncThunkArgOnly<TArg1,TArg2> Bind(TArg1 arg1,TArg2 arg2) => new(actionName, work, arg1,arg2);
}

public readonly struct AsyncThunk<TArg1,TArg2, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2, Task<TResult>> work)
{
	public CallableAsyncThunk<TArg1,TArg2, TResult> Bind(TArg1 arg1,TArg2 arg2) => new(actionName, work, arg1,arg2);
}

public readonly struct CallableAsyncThunkArgOnly<TArg1,TArg2>(string actionName, Func<ThunkApi, TArg1,TArg2, Task> work, TArg1 arg1,TArg2 arg2) : ICallableAsyncThunk
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			await work(api, arg1,arg2);
			api.Dispatch(new ThunkFulfilled(actionName));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}

public readonly struct CallableAsyncThunk<TArg1,TArg2, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2, Task<TResult>> work, TArg1 arg1,TArg2 arg2) : ICallableAsyncThunk<TResult>
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			var result = await work(api, arg1,arg2);
			api.Dispatch(new ThunkFulfilled<TResult>(actionName, result));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}


public readonly struct AsyncThunkArgOnly<TArg1,TArg2,TArg3>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3, Task> work)
{
	public CallableAsyncThunkArgOnly<TArg1,TArg2,TArg3> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3) => new(actionName, work, arg1,arg2,arg3);
}

public readonly struct AsyncThunk<TArg1,TArg2,TArg3, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3, Task<TResult>> work)
{
	public CallableAsyncThunk<TArg1,TArg2,TArg3, TResult> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3) => new(actionName, work, arg1,arg2,arg3);
}

public readonly struct CallableAsyncThunkArgOnly<TArg1,TArg2,TArg3>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3, Task> work, TArg1 arg1,TArg2 arg2,TArg3 arg3) : ICallableAsyncThunk
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			await work(api, arg1,arg2,arg3);
			api.Dispatch(new ThunkFulfilled(actionName));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}

public readonly struct CallableAsyncThunk<TArg1,TArg2,TArg3, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3, Task<TResult>> work, TArg1 arg1,TArg2 arg2,TArg3 arg3) : ICallableAsyncThunk<TResult>
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			var result = await work(api, arg1,arg2,arg3);
			api.Dispatch(new ThunkFulfilled<TResult>(actionName, result));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}


public readonly struct AsyncThunkArgOnly<TArg1,TArg2,TArg3,TArg4>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4, Task> work)
{
	public CallableAsyncThunkArgOnly<TArg1,TArg2,TArg3,TArg4> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4) => new(actionName, work, arg1,arg2,arg3,arg4);
}

public readonly struct AsyncThunk<TArg1,TArg2,TArg3,TArg4, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4, Task<TResult>> work)
{
	public CallableAsyncThunk<TArg1,TArg2,TArg3,TArg4, TResult> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4) => new(actionName, work, arg1,arg2,arg3,arg4);
}

public readonly struct CallableAsyncThunkArgOnly<TArg1,TArg2,TArg3,TArg4>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4, Task> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4) : ICallableAsyncThunk
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			await work(api, arg1,arg2,arg3,arg4);
			api.Dispatch(new ThunkFulfilled(actionName));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}

public readonly struct CallableAsyncThunk<TArg1,TArg2,TArg3,TArg4, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4, Task<TResult>> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4) : ICallableAsyncThunk<TResult>
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			var result = await work(api, arg1,arg2,arg3,arg4);
			api.Dispatch(new ThunkFulfilled<TResult>(actionName, result));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}


public readonly struct AsyncThunkArgOnly<TArg1,TArg2,TArg3,TArg4,TArg5>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task> work)
{
	public CallableAsyncThunkArgOnly<TArg1,TArg2,TArg3,TArg4,TArg5> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5) => new(actionName, work, arg1,arg2,arg3,arg4,arg5);
}

public readonly struct AsyncThunk<TArg1,TArg2,TArg3,TArg4,TArg5, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task<TResult>> work)
{
	public CallableAsyncThunk<TArg1,TArg2,TArg3,TArg4,TArg5, TResult> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5) => new(actionName, work, arg1,arg2,arg3,arg4,arg5);
}

public readonly struct CallableAsyncThunkArgOnly<TArg1,TArg2,TArg3,TArg4,TArg5>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5) : ICallableAsyncThunk
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			await work(api, arg1,arg2,arg3,arg4,arg5);
			api.Dispatch(new ThunkFulfilled(actionName));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}

public readonly struct CallableAsyncThunk<TArg1,TArg2,TArg3,TArg4,TArg5, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task<TResult>> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5) : ICallableAsyncThunk<TResult>
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			var result = await work(api, arg1,arg2,arg3,arg4,arg5);
			api.Dispatch(new ThunkFulfilled<TResult>(actionName, result));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}


public readonly struct AsyncThunkArgOnly<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task> work)
{
	public CallableAsyncThunkArgOnly<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6) => new(actionName, work, arg1,arg2,arg3,arg4,arg5,arg6);
}

public readonly struct AsyncThunk<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task<TResult>> work)
{
	public CallableAsyncThunk<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, TResult> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6) => new(actionName, work, arg1,arg2,arg3,arg4,arg5,arg6);
}

public readonly struct CallableAsyncThunkArgOnly<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6) : ICallableAsyncThunk
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			await work(api, arg1,arg2,arg3,arg4,arg5,arg6);
			api.Dispatch(new ThunkFulfilled(actionName));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}

public readonly struct CallableAsyncThunk<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task<TResult>> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6) : ICallableAsyncThunk<TResult>
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			var result = await work(api, arg1,arg2,arg3,arg4,arg5,arg6);
			api.Dispatch(new ThunkFulfilled<TResult>(actionName, result));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}


public readonly struct AsyncThunkArgOnly<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task> work)
{
	public CallableAsyncThunkArgOnly<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6,TArg7 arg7) => new(actionName, work, arg1,arg2,arg3,arg4,arg5,arg6,arg7);
}

public readonly struct AsyncThunk<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task<TResult>> work)
{
	public CallableAsyncThunk<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, TResult> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6,TArg7 arg7) => new(actionName, work, arg1,arg2,arg3,arg4,arg5,arg6,arg7);
}

public readonly struct CallableAsyncThunkArgOnly<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6,TArg7 arg7) : ICallableAsyncThunk
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			await work(api, arg1,arg2,arg3,arg4,arg5,arg6,arg7);
			api.Dispatch(new ThunkFulfilled(actionName));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}

public readonly struct CallableAsyncThunk<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task<TResult>> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6,TArg7 arg7) : ICallableAsyncThunk<TResult>
{
	public async Task ExecuteAsync(ThunkApi api)
	{
		try
		{
			api.Dispatch(new ThunkPending(actionName));
			var result = await work(api, arg1,arg2,arg3,arg4,arg5,arg6,arg7);
			api.Dispatch(new ThunkFulfilled<TResult>(actionName, result));
		}
		catch (Exception e)
		{
			api.Dispatch(new ThunkRejected(actionName, e));
		}
	}
}

