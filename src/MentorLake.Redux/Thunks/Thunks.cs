
namespace MentorLake.Redux.Thunks;

public readonly struct ThunkAction(string actionName, Func<ThunkApi, Task> work)
{
	private readonly CallableThunkAction _thunk = new(actionName, work);
	public CallableThunkAction Bind() => _thunk;
}

public readonly struct ThunkFunc<TResult>(string actionName, Func<ThunkApi, Task<TResult>> work)
{
	private readonly CallableThunkFunc<TResult> _thunk = new(actionName, work);
	public CallableThunkFunc<TResult> Bind() => _thunk;
}

public readonly struct CallableThunkAction(string actionName, Func<ThunkApi, Task> work) : ICallableThunkAction
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

public readonly struct CallableThunkFunc<TResult>(string actionName, Func<ThunkApi, Task<TResult>> work) : ICallableThunkFunc<TResult>
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

public readonly struct ThunkAction<TArg1>(string actionName, Func<ThunkApi, TArg1, Task> work)
{
	public CallableThunkAction<TArg1> Bind(TArg1 arg1) => new(actionName, work, arg1);
}

public readonly struct ThunkFunc<TArg1, TResult>(string actionName, Func<ThunkApi, TArg1, Task<TResult>> work)
{
	public CallableThunkFunc<TArg1, TResult> Bind(TArg1 arg1) => new(actionName, work, arg1);
}

public readonly struct CallableThunkAction<TArg1>(string actionName, Func<ThunkApi, TArg1, Task> work, TArg1 arg1) : ICallableThunkAction
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

public readonly struct CallableThunkFunc<TArg1, TResult>(string actionName, Func<ThunkApi, TArg1, Task<TResult>> work, TArg1 arg1) : ICallableThunkFunc<TResult>
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


public readonly struct ThunkAction<TArg1,TArg2>(string actionName, Func<ThunkApi, TArg1,TArg2, Task> work)
{
	public CallableThunkAction<TArg1,TArg2> Bind(TArg1 arg1,TArg2 arg2) => new(actionName, work, arg1,arg2);
}

public readonly struct ThunkFunc<TArg1,TArg2, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2, Task<TResult>> work)
{
	public CallableThunkFunc<TArg1,TArg2, TResult> Bind(TArg1 arg1,TArg2 arg2) => new(actionName, work, arg1,arg2);
}

public readonly struct CallableThunkAction<TArg1,TArg2>(string actionName, Func<ThunkApi, TArg1,TArg2, Task> work, TArg1 arg1,TArg2 arg2) : ICallableThunkAction
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

public readonly struct CallableThunkFunc<TArg1,TArg2, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2, Task<TResult>> work, TArg1 arg1,TArg2 arg2) : ICallableThunkFunc<TResult>
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


public readonly struct ThunkAction<TArg1,TArg2,TArg3>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3, Task> work)
{
	public CallableThunkAction<TArg1,TArg2,TArg3> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3) => new(actionName, work, arg1,arg2,arg3);
}

public readonly struct ThunkFunc<TArg1,TArg2,TArg3, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3, Task<TResult>> work)
{
	public CallableThunkFunc<TArg1,TArg2,TArg3, TResult> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3) => new(actionName, work, arg1,arg2,arg3);
}

public readonly struct CallableThunkAction<TArg1,TArg2,TArg3>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3, Task> work, TArg1 arg1,TArg2 arg2,TArg3 arg3) : ICallableThunkAction
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

public readonly struct CallableThunkFunc<TArg1,TArg2,TArg3, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3, Task<TResult>> work, TArg1 arg1,TArg2 arg2,TArg3 arg3) : ICallableThunkFunc<TResult>
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


public readonly struct ThunkAction<TArg1,TArg2,TArg3,TArg4>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4, Task> work)
{
	public CallableThunkAction<TArg1,TArg2,TArg3,TArg4> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4) => new(actionName, work, arg1,arg2,arg3,arg4);
}

public readonly struct ThunkFunc<TArg1,TArg2,TArg3,TArg4, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4, Task<TResult>> work)
{
	public CallableThunkFunc<TArg1,TArg2,TArg3,TArg4, TResult> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4) => new(actionName, work, arg1,arg2,arg3,arg4);
}

public readonly struct CallableThunkAction<TArg1,TArg2,TArg3,TArg4>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4, Task> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4) : ICallableThunkAction
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

public readonly struct CallableThunkFunc<TArg1,TArg2,TArg3,TArg4, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4, Task<TResult>> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4) : ICallableThunkFunc<TResult>
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


public readonly struct ThunkAction<TArg1,TArg2,TArg3,TArg4,TArg5>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task> work)
{
	public CallableThunkAction<TArg1,TArg2,TArg3,TArg4,TArg5> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5) => new(actionName, work, arg1,arg2,arg3,arg4,arg5);
}

public readonly struct ThunkFunc<TArg1,TArg2,TArg3,TArg4,TArg5, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task<TResult>> work)
{
	public CallableThunkFunc<TArg1,TArg2,TArg3,TArg4,TArg5, TResult> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5) => new(actionName, work, arg1,arg2,arg3,arg4,arg5);
}

public readonly struct CallableThunkAction<TArg1,TArg2,TArg3,TArg4,TArg5>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5) : ICallableThunkAction
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

public readonly struct CallableThunkFunc<TArg1,TArg2,TArg3,TArg4,TArg5, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task<TResult>> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5) : ICallableThunkFunc<TResult>
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


public readonly struct ThunkAction<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task> work)
{
	public CallableThunkAction<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6) => new(actionName, work, arg1,arg2,arg3,arg4,arg5,arg6);
}

public readonly struct ThunkFunc<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task<TResult>> work)
{
	public CallableThunkFunc<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, TResult> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6) => new(actionName, work, arg1,arg2,arg3,arg4,arg5,arg6);
}

public readonly struct CallableThunkAction<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6) : ICallableThunkAction
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

public readonly struct CallableThunkFunc<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task<TResult>> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6) : ICallableThunkFunc<TResult>
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


public readonly struct ThunkAction<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task> work)
{
	public CallableThunkAction<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6,TArg7 arg7) => new(actionName, work, arg1,arg2,arg3,arg4,arg5,arg6,arg7);
}

public readonly struct ThunkFunc<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task<TResult>> work)
{
	public CallableThunkFunc<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, TResult> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6,TArg7 arg7) => new(actionName, work, arg1,arg2,arg3,arg4,arg5,arg6,arg7);
}

public readonly struct CallableThunkAction<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6,TArg7 arg7) : ICallableThunkAction
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

public readonly struct CallableThunkFunc<TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, TResult>(string actionName, Func<ThunkApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task<TResult>> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6,TArg7 arg7) : ICallableThunkFunc<TResult>
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

