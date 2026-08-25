
namespace MentorLake.Redux.Thunks;


public readonly struct ThunkAction<TApi>(string actionName, Func<TApi, Task> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	private readonly CallableThunkAction<TApi> _thunk = new(actionName ?? string.Empty, work);
	public CallableThunkAction<TApi> Bind() => _thunk;
	public ThunkAction(Func<TApi, Task> work) : this(string.Empty, work) { }
	public static implicit operator ThunkAction<TApi>(Func<TApi, Task> work) => new(work);
}

public readonly struct ThunkFunc<TApi,TResult>(string actionName, Func<TApi, Task<TResult>> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	private readonly CallableThunkFunc<TApi, TResult> _thunk = new(actionName ?? string.Empty, work);
	public CallableThunkFunc<TApi, TResult> Bind() => _thunk;
	public ThunkFunc(Func<TApi, Task<TResult>> work) : this(string.Empty, work) { }
	public static implicit operator ThunkFunc<TApi, TResult>(Func<TApi, Task<TResult>> work) => new(work);
}

public readonly struct CallableThunkAction<TApi>(string actionName, Func<TApi, Task> work) : ICallableThunkAction<TApi> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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

	public static implicit operator CallableThunkAction<TApi>(Func<TApi, Task> work) => new(string.Empty, work);
}

public readonly struct CallableThunkFunc<TApi, TResult>(string actionName, Func<TApi, Task<TResult>> work) : ICallableThunkFunc<TApi, TResult> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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

	public static implicit operator CallableThunkFunc<TApi, TResult>(Func<TApi, Task<TResult>> work) => new(string.Empty, work);
}

public readonly struct ThunkAction<TApi, TArg1>(string actionName, Func<TApi, TArg1, Task> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	public ThunkAction(Func<TApi, TArg1, Task> work) : this(string.Empty, work) { }
	public CallableThunkAction<TApi, TArg1> Bind(TArg1 arg1) => new(actionName ?? string.Empty, work, arg1);
	public static implicit operator ThunkAction<TApi, TArg1>(Func<TApi, TArg1, Task> work) => new(work);
}

public readonly struct ThunkFunc<TApi, TArg1, TResult>(string actionName, Func<TApi, TArg1, Task<TResult>> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	public ThunkFunc(Func<TApi, TArg1, Task<TResult>> work) : this(string.Empty, work) { }
	public CallableThunkFunc<TApi, TArg1, TResult> Bind(TArg1 arg1) => new(actionName ?? string.Empty, work, arg1);
	public static implicit operator ThunkFunc<TApi, TArg1, TResult>(Func<TApi, TArg1, Task<TResult>> work) => new(work);
}

public readonly struct CallableThunkAction<TApi, TArg1>(string actionName, Func<TApi, TArg1, Task> work, TArg1 arg1) : ICallableThunkAction<TApi> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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

public readonly struct CallableThunkFunc<TApi, TArg1, TResult>(string actionName, Func<TApi, TArg1, Task<TResult>> work, TArg1 arg1) : ICallableThunkFunc<TApi, TResult> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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


public readonly struct ThunkAction<TApi, TArg1,TArg2>(string actionName, Func<TApi, TArg1,TArg2, Task> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	public ThunkAction(Func<TApi, TArg1,TArg2, Task> work) : this(string.Empty, work) { }
	public CallableThunkAction<TApi, TArg1,TArg2> Bind(TArg1 arg1,TArg2 arg2) => new(actionName ?? string.Empty, work, arg1,arg2);
	public static implicit operator ThunkAction<TApi, TArg1,TArg2>(Func<TApi, TArg1,TArg2, Task> work) => new(work);
}

public readonly struct ThunkFunc<TApi, TArg1,TArg2, TResult>(string actionName, Func<TApi, TArg1,TArg2, Task<TResult>> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	public ThunkFunc(Func<TApi, TArg1,TArg2, Task<TResult>> work) : this(string.Empty, work) { }
	public CallableThunkFunc<TApi, TArg1,TArg2, TResult> Bind(TArg1 arg1,TArg2 arg2) => new(actionName ?? string.Empty, work, arg1,arg2);
	public static implicit operator ThunkFunc<TApi, TArg1,TArg2, TResult>(Func<TApi, TArg1,TArg2, Task<TResult>> work) => new(work);
}

public readonly struct CallableThunkAction<TApi, TArg1,TArg2>(string actionName, Func<TApi, TArg1,TArg2, Task> work, TArg1 arg1,TArg2 arg2) : ICallableThunkAction<TApi> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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

public readonly struct CallableThunkFunc<TApi, TArg1,TArg2, TResult>(string actionName, Func<TApi, TArg1,TArg2, Task<TResult>> work, TArg1 arg1,TArg2 arg2) : ICallableThunkFunc<TApi, TResult> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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


public readonly struct ThunkAction<TApi, TArg1,TArg2,TArg3>(string actionName, Func<TApi, TArg1,TArg2,TArg3, Task> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	public ThunkAction(Func<TApi, TArg1,TArg2,TArg3, Task> work) : this(string.Empty, work) { }
	public CallableThunkAction<TApi, TArg1,TArg2,TArg3> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3) => new(actionName ?? string.Empty, work, arg1,arg2,arg3);
	public static implicit operator ThunkAction<TApi, TArg1,TArg2,TArg3>(Func<TApi, TArg1,TArg2,TArg3, Task> work) => new(work);
}

public readonly struct ThunkFunc<TApi, TArg1,TArg2,TArg3, TResult>(string actionName, Func<TApi, TArg1,TArg2,TArg3, Task<TResult>> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	public ThunkFunc(Func<TApi, TArg1,TArg2,TArg3, Task<TResult>> work) : this(string.Empty, work) { }
	public CallableThunkFunc<TApi, TArg1,TArg2,TArg3, TResult> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3) => new(actionName ?? string.Empty, work, arg1,arg2,arg3);
	public static implicit operator ThunkFunc<TApi, TArg1,TArg2,TArg3, TResult>(Func<TApi, TArg1,TArg2,TArg3, Task<TResult>> work) => new(work);
}

public readonly struct CallableThunkAction<TApi, TArg1,TArg2,TArg3>(string actionName, Func<TApi, TArg1,TArg2,TArg3, Task> work, TArg1 arg1,TArg2 arg2,TArg3 arg3) : ICallableThunkAction<TApi> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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

public readonly struct CallableThunkFunc<TApi, TArg1,TArg2,TArg3, TResult>(string actionName, Func<TApi, TArg1,TArg2,TArg3, Task<TResult>> work, TArg1 arg1,TArg2 arg2,TArg3 arg3) : ICallableThunkFunc<TApi, TResult> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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


public readonly struct ThunkAction<TApi, TArg1,TArg2,TArg3,TArg4>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4, Task> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	public ThunkAction(Func<TApi, TArg1,TArg2,TArg3,TArg4, Task> work) : this(string.Empty, work) { }
	public CallableThunkAction<TApi, TArg1,TArg2,TArg3,TArg4> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4) => new(actionName ?? string.Empty, work, arg1,arg2,arg3,arg4);
	public static implicit operator ThunkAction<TApi, TArg1,TArg2,TArg3,TArg4>(Func<TApi, TArg1,TArg2,TArg3,TArg4, Task> work) => new(work);
}

public readonly struct ThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4, TResult>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4, Task<TResult>> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	public ThunkFunc(Func<TApi, TArg1,TArg2,TArg3,TArg4, Task<TResult>> work) : this(string.Empty, work) { }
	public CallableThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4, TResult> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4) => new(actionName ?? string.Empty, work, arg1,arg2,arg3,arg4);
	public static implicit operator ThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4, TResult>(Func<TApi, TArg1,TArg2,TArg3,TArg4, Task<TResult>> work) => new(work);
}

public readonly struct CallableThunkAction<TApi, TArg1,TArg2,TArg3,TArg4>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4, Task> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4) : ICallableThunkAction<TApi> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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

public readonly struct CallableThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4, TResult>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4, Task<TResult>> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4) : ICallableThunkFunc<TApi, TResult> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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


public readonly struct ThunkAction<TApi, TArg1,TArg2,TArg3,TArg4,TArg5>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	public ThunkAction(Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task> work) : this(string.Empty, work) { }
	public CallableThunkAction<TApi, TArg1,TArg2,TArg3,TArg4,TArg5> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5) => new(actionName ?? string.Empty, work, arg1,arg2,arg3,arg4,arg5);
	public static implicit operator ThunkAction<TApi, TArg1,TArg2,TArg3,TArg4,TArg5>(Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task> work) => new(work);
}

public readonly struct ThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4,TArg5, TResult>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task<TResult>> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	public ThunkFunc(Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task<TResult>> work) : this(string.Empty, work) { }
	public CallableThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4,TArg5, TResult> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5) => new(actionName ?? string.Empty, work, arg1,arg2,arg3,arg4,arg5);
	public static implicit operator ThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4,TArg5, TResult>(Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task<TResult>> work) => new(work);
}

public readonly struct CallableThunkAction<TApi, TArg1,TArg2,TArg3,TArg4,TArg5>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5) : ICallableThunkAction<TApi> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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

public readonly struct CallableThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4,TArg5, TResult>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5, Task<TResult>> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5) : ICallableThunkFunc<TApi, TResult> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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


public readonly struct ThunkAction<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	public ThunkAction(Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task> work) : this(string.Empty, work) { }
	public CallableThunkAction<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6) => new(actionName ?? string.Empty, work, arg1,arg2,arg3,arg4,arg5,arg6);
	public static implicit operator ThunkAction<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6>(Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task> work) => new(work);
}

public readonly struct ThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, TResult>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task<TResult>> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	public ThunkFunc(Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task<TResult>> work) : this(string.Empty, work) { }
	public CallableThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, TResult> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6) => new(actionName ?? string.Empty, work, arg1,arg2,arg3,arg4,arg5,arg6);
	public static implicit operator ThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, TResult>(Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task<TResult>> work) => new(work);
}

public readonly struct CallableThunkAction<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6) : ICallableThunkAction<TApi> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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

public readonly struct CallableThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, TResult>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6, Task<TResult>> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6) : ICallableThunkFunc<TApi, TResult> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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


public readonly struct ThunkAction<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	public ThunkAction(Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task> work) : this(string.Empty, work) { }
	public CallableThunkAction<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6,TArg7 arg7) => new(actionName ?? string.Empty, work, arg1,arg2,arg3,arg4,arg5,arg6,arg7);
	public static implicit operator ThunkAction<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7>(Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task> work) => new(work);
}

public readonly struct ThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, TResult>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task<TResult>> work) where TApi : ThunkApi
{
	public string ActionName => actionName;
	public string PendingName => $"{actionName}/pending";
	public string FulfilledName => $"{actionName}/fulfilled";
	public string RejectedName => $"{actionName}/rejected";
	public ThunkFunc(Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task<TResult>> work) : this(string.Empty, work) { }
	public CallableThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, TResult> Bind(TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6,TArg7 arg7) => new(actionName ?? string.Empty, work, arg1,arg2,arg3,arg4,arg5,arg6,arg7);
	public static implicit operator ThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, TResult>(Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task<TResult>> work) => new(work);
}

public readonly struct CallableThunkAction<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6,TArg7 arg7) : ICallableThunkAction<TApi> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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

public readonly struct CallableThunkFunc<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, TResult>(string actionName, Func<TApi, TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7, Task<TResult>> work, TArg1 arg1,TArg2 arg2,TArg3 arg3,TArg4 arg4,TArg5 arg5,TArg6 arg6,TArg7 arg7) : ICallableThunkFunc<TApi, TResult> where TApi : ThunkApi
{
	public async Task ExecuteAsync(TApi api)
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

