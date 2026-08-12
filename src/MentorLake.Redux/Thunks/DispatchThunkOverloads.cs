
using MentorLake.Redux.Thunks;

namespace MentorLake.Redux
{
	public sealed partial class ReduxStore
	{
		public ThunkResult DispatchThunk<TArg1>(Func<ThunkApi, TArg1, Task> work, TArg1 arg1)
		{
			return DispatchThunk(new CallableThunkAction<TArg1>(string.Empty, work, arg1));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TResult>(Func<ThunkApi, TArg1, Task<TResult>> work, TArg1 arg1)
		{
			return DispatchThunk(new CallableThunkFunc<TArg1, TResult>(string.Empty, work, arg1));
		}

		public ThunkResult DispatchThunk<TArg1, TArg2>(Func<ThunkApi, TArg1, TArg2, Task> work, TArg1 arg1, TArg2 arg2)
		{
			return DispatchThunk(new CallableThunkAction<TArg1, TArg2>(string.Empty, work, arg1, arg2));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TResult>(Func<ThunkApi, TArg1, TArg2, Task<TResult>> work, TArg1 arg1, TArg2 arg2)
		{
			return DispatchThunk(new CallableThunkFunc<TArg1, TArg2, TResult>(string.Empty, work, arg1, arg2));
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3>(Func<ThunkApi, TArg1, TArg2, TArg3, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return DispatchThunk(new CallableThunkAction<TArg1, TArg2, TArg3>(string.Empty, work, arg1, arg2, arg3));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return DispatchThunk(new CallableThunkFunc<TArg1, TArg2, TArg3, TResult>(string.Empty, work, arg1, arg2, arg3));
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return DispatchThunk(new CallableThunkAction<TArg1, TArg2, TArg3, TArg4>(string.Empty, work, arg1, arg2, arg3, arg4));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return DispatchThunk(new CallableThunkFunc<TArg1, TArg2, TArg3, TArg4, TResult>(string.Empty, work, arg1, arg2, arg3, arg4));
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return DispatchThunk(new CallableThunkAction<TArg1, TArg2, TArg3, TArg4, TArg5>(string.Empty, work, arg1, arg2, arg3, arg4, arg5));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return DispatchThunk(new CallableThunkFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string.Empty, work, arg1, arg2, arg3, arg4, arg5));
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return DispatchThunk(new CallableThunkAction<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return DispatchThunk(new CallableThunkFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6));
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return DispatchThunk(new CallableThunkAction<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6, arg7));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return DispatchThunk(new CallableThunkFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6, arg7));
		}

	}
}

namespace MentorLake.Redux.Thunks
{
	public partial class ThunkApi
	{
		public ThunkResult DispatchThunk<TArg1>(Func<ThunkApi, TArg1, Task> work, TArg1 arg1)
		{
			return store.DispatchThunk(work, arg1);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TResult>(Func<ThunkApi, TArg1, Task<TResult>> work, TArg1 arg1)
		{
			return store.DispatchThunk(work, arg1);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2>(Func<ThunkApi, TArg1, TArg2, Task> work, TArg1 arg1, TArg2 arg2)
		{
			return store.DispatchThunk(work, arg1, arg2);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TResult>(Func<ThunkApi, TArg1, TArg2, Task<TResult>> work, TArg1 arg1, TArg2 arg2)
		{
			return store.DispatchThunk(work, arg1, arg2);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3>(Func<ThunkApi, TArg1, TArg2, TArg3, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return store.DispatchThunk(work, arg1, arg2, arg3);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return store.DispatchThunk(work, arg1, arg2, arg3);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return store.DispatchThunk(work, arg1, arg2, arg3, arg4);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return store.DispatchThunk(work, arg1, arg2, arg3, arg4);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return store.DispatchThunk(work, arg1, arg2, arg3, arg4, arg5);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return store.DispatchThunk(work, arg1, arg2, arg3, arg4, arg5);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return store.DispatchThunk(work, arg1, arg2, arg3, arg4, arg5, arg6);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return store.DispatchThunk(work, arg1, arg2, arg3, arg4, arg5, arg6);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return store.DispatchThunk(work, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return store.DispatchThunk(work, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}

	}
}
