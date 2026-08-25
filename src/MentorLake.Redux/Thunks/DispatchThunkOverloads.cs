
using MentorLake.Redux.Thunks;

namespace MentorLake.Redux
{
	public sealed partial class ReduxStore
	{
		public ThunkResult DispatchThunk(ThunkAction<ThunkApi> thunk)
		{
			return DispatchThunk(thunk.Bind());
		}

		public ThunkResult DispatchThunk(Func<ThunkApi, Task> thunk)
		{
			return DispatchThunk(new ThunkAction<ThunkApi>(string.Empty, thunk).Bind());
		}

		public ThunkResult<TResult> DispatchThunk<TResult>(ThunkFunc<ThunkApi, TResult> thunk)
		{
			return DispatchThunk(thunk.Bind());
		}

		public ThunkResult<TResult> DispatchThunk<TResult>(Func<ThunkApi, Task<TResult>> thunk)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TResult>(string.Empty, thunk).Bind());
		}

		public ThunkResult DispatchThunk<TArg1>(Func<ThunkApi, TArg1, Task> work, TArg1 arg1)
		{
			return DispatchThunk(new ThunkAction<ThunkApi, TArg1>(string.Empty, work).Bind(arg1));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TResult>(Func<ThunkApi, TArg1, Task<TResult>> work, TArg1 arg1)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TArg1, TResult>(string.Empty, work).Bind(arg1));
		}

		public ThunkResult DispatchThunk<TArg1>(ThunkAction<ThunkApi, TArg1> thunk, TArg1 arg1)
		{
			return DispatchThunk(thunk.Bind(arg1));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TResult>(ThunkFunc<ThunkApi, TArg1, TResult> thunk, TArg1 arg1)
		{
			return DispatchThunk(thunk.Bind(arg1));
		}
		public ThunkResult DispatchThunk<TArg1, TArg2>(Func<ThunkApi, TArg1, TArg2, Task> work, TArg1 arg1, TArg2 arg2)
		{
			return DispatchThunk(new ThunkAction<ThunkApi, TArg1, TArg2>(string.Empty, work).Bind(arg1, arg2));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TResult>(Func<ThunkApi, TArg1, TArg2, Task<TResult>> work, TArg1 arg1, TArg2 arg2)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TArg1, TArg2, TResult>(string.Empty, work).Bind(arg1, arg2));
		}

		public ThunkResult DispatchThunk<TArg1, TArg2>(ThunkAction<ThunkApi, TArg1, TArg2> thunk, TArg1 arg1, TArg2 arg2)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TResult> thunk, TArg1 arg1, TArg2 arg2)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2));
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3>(Func<ThunkApi, TArg1, TArg2, TArg3, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return DispatchThunk(new ThunkAction<ThunkApi, TArg1, TArg2, TArg3>(string.Empty, work).Bind(arg1, arg2, arg3));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TResult>(string.Empty, work).Bind(arg1, arg2, arg3));
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3));
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return DispatchThunk(new ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4>(string.Empty, work).Bind(arg1, arg2, arg3, arg4));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TResult>(string.Empty, work).Bind(arg1, arg2, arg3, arg4));
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4));
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return DispatchThunk(new ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5));
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5));
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return DispatchThunk(new ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6));
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6));
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return DispatchThunk(new ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
		}
	}
}

namespace MentorLake.Redux.Thunks
{
	public partial class ThunkApi
	{
		public ThunkResult DispatchThunk<TApi>(ICallableThunkAction<TApi> thunk) where TApi : ThunkApi
		{
			return store.DispatchThunk(thunk);
		}

		public ThunkResult<TResult> DispatchThunk<TApi, TResult>(ICallableThunkFunc<TApi, TResult> thunk) where TApi : ThunkApi
		{
			return store.DispatchThunk(thunk);
		}

		public ThunkResult DispatchThunk(ThunkAction<ThunkApi> thunk)
		{
			return Using<ThunkApi>().Dispatch(thunk);
		}

		public ThunkResult DispatchThunk(Func<ThunkApi, Task> thunk)
		{
			return Using<ThunkApi>().Dispatch(new ThunkAction<ThunkApi>(string.Empty, thunk));
		}

		public ThunkResult<TResult> DispatchThunk<TResult>(ThunkFunc<ThunkApi, TResult> thunk)
		{
			return Using<ThunkApi>().Dispatch(thunk);
		}

		public ThunkResult<TResult> DispatchThunk<TResult>(Func<ThunkApi, Task<TResult>> thunk)
		{
			return Using<ThunkApi>().Dispatch(new ThunkFunc<ThunkApi, TResult>(string.Empty, thunk));
		}

		public ThunkResult DispatchThunk<TArg1>(Func<ThunkApi, TArg1, Task> work, TArg1 arg1)
		{
			return Using<ThunkApi>().Dispatch(work, arg1);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TResult>(Func<ThunkApi, TArg1, Task<TResult>> work, TArg1 arg1)
		{
			return Using<ThunkApi>().Dispatch(work, arg1);
		}

		public ThunkResult DispatchThunk<TArg1>(ThunkAction<ThunkApi, TArg1> thunk, TArg1 arg1)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TResult>(ThunkFunc<ThunkApi, TArg1, TResult> thunk, TArg1 arg1)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2>(Func<ThunkApi, TArg1, TArg2, Task> work, TArg1 arg1, TArg2 arg2)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TResult>(Func<ThunkApi, TArg1, TArg2, Task<TResult>> work, TArg1 arg1, TArg2 arg2)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2>(ThunkAction<ThunkApi, TArg1, TArg2> thunk, TArg1 arg1, TArg2 arg2)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TResult> thunk, TArg1 arg1, TArg2 arg2)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3>(Func<ThunkApi, TArg1, TArg2, TArg3, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4, arg5);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4, arg5);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4, arg5);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4, arg5);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4, arg5, arg6);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4, arg5, arg6);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4, arg5, arg6);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4, arg5, arg6);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}
	}

	public sealed partial class ThunkDispatcher<TApi> where TApi : ThunkApi
	{
		public ThunkResult Dispatch(CallableThunkAction<TApi> thunk) => _store.DispatchThunk(thunk);
		public ThunkResult Dispatch(ThunkAction<TApi> thunk) => Dispatch(thunk.Bind());
		public ThunkResult Dispatch(string actionName, Func<TApi, Task> work) => Dispatch(new ThunkAction<TApi>(actionName ?? string.Empty, work).Bind());
		public ThunkResult Dispatch(Func<TApi, Task> work) => Dispatch(string.Empty, work);

		public ThunkResult<TResult> Dispatch<TResult>(CallableThunkFunc<TApi, TResult> thunk) => _store.DispatchThunk(thunk);
		public ThunkResult<TResult> Dispatch<TResult>(ThunkFunc<TApi, TResult> thunk) => Dispatch(thunk.Bind());
		public ThunkResult<TResult> Dispatch<TResult>(string actionName, Func<TApi, Task<TResult>> work) => Dispatch(new ThunkFunc<TApi, TResult>(actionName ?? string.Empty, work));
		public ThunkResult<TResult> Dispatch<TResult>(Func<TApi, Task<TResult>> work) => Dispatch(string.Empty, work);
		public ThunkResult Dispatch<TArg1>(ThunkAction<TApi, TArg1> thunk, TArg1 arg1)
		{
			return _store.DispatchThunk(thunk.Bind(arg1));
		}

		public ThunkResult Dispatch<TArg1>(Func<TApi, TArg1, Task> work, TArg1 arg1)
		{
			return Dispatch(string.Empty, work, arg1);
		}

		public ThunkResult Dispatch<TArg1>(string actionName, Func<TApi, TArg1, Task> work, TArg1 arg1)
		{
			return _store.DispatchThunk(new ThunkAction<TApi, TArg1>(actionName, work).Bind(arg1));
		}

		public ThunkResult<TResult> Dispatch<TArg1, TResult>(ThunkFunc<TApi, TArg1, TResult> thunk, TArg1 arg1)
		{
			return _store.DispatchThunk(thunk.Bind(arg1));
		}

		public ThunkResult<TResult> Dispatch<TArg1, TResult>(Func<TApi, TArg1, Task<TResult>> work, TArg1 arg1)
		{
			return Dispatch(string.Empty, work, arg1);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TResult>(string actionName, Func<TApi, TArg1, Task<TResult>> work, TArg1 arg1)
		{
			return _store.DispatchThunk(new ThunkFunc<TApi, TArg1, TResult>(actionName, work).Bind(arg1));
		}
		public ThunkResult Dispatch<TArg1, TArg2>(ThunkAction<TApi, TArg1, TArg2> thunk, TArg1 arg1, TArg2 arg2)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2));
		}

		public ThunkResult Dispatch<TArg1, TArg2>(Func<TApi, TArg1, TArg2, Task> work, TArg1 arg1, TArg2 arg2)
		{
			return Dispatch(string.Empty, work, arg1, arg2);
		}

		public ThunkResult Dispatch<TArg1, TArg2>(string actionName, Func<TApi, TArg1, TArg2, Task> work, TArg1 arg1, TArg2 arg2)
		{
			return _store.DispatchThunk(new ThunkAction<TApi, TArg1, TArg2>(actionName, work).Bind(arg1, arg2));
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TResult>(ThunkFunc<TApi, TArg1, TArg2, TResult> thunk, TArg1 arg1, TArg2 arg2)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2));
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TResult>(Func<TApi, TArg1, TArg2, Task<TResult>> work, TArg1 arg1, TArg2 arg2)
		{
			return Dispatch(string.Empty, work, arg1, arg2);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TResult>(string actionName, Func<TApi, TArg1, TArg2, Task<TResult>> work, TArg1 arg1, TArg2 arg2)
		{
			return _store.DispatchThunk(new ThunkFunc<TApi, TArg1, TArg2, TResult>(actionName, work).Bind(arg1, arg2));
		}
		public ThunkResult Dispatch<TArg1, TArg2, TArg3>(ThunkAction<TApi, TArg1, TArg2, TArg3> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3));
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3>(Func<TApi, TArg1, TArg2, TArg3, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3);
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3>(string actionName, Func<TApi, TArg1, TArg2, TArg3, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return _store.DispatchThunk(new ThunkAction<TApi, TArg1, TArg2, TArg3>(actionName, work).Bind(arg1, arg2, arg3));
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TResult>(ThunkFunc<TApi, TArg1, TArg2, TArg3, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3));
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TResult>(Func<TApi, TArg1, TArg2, TArg3, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TResult>(string actionName, Func<TApi, TArg1, TArg2, TArg3, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return _store.DispatchThunk(new ThunkFunc<TApi, TArg1, TArg2, TArg3, TResult>(actionName, work).Bind(arg1, arg2, arg3));
		}
		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4>(ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4));
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4>(Func<TApi, TArg1, TArg2, TArg3, TArg4, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4);
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return _store.DispatchThunk(new ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4>(actionName, work).Bind(arg1, arg2, arg3, arg4));
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TResult>(ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4));
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TResult>(Func<TApi, TArg1, TArg2, TArg3, TArg4, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TResult>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return _store.DispatchThunk(new ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TResult>(actionName, work).Bind(arg1, arg2, arg3, arg4));
		}
		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5>(ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5));
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5>(Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4, arg5);
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return _store.DispatchThunk(new ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5));
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5));
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4, arg5);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return _store.DispatchThunk(new ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5));
		}
		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6));
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6);
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return _store.DispatchThunk(new ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6));
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6));
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			return _store.DispatchThunk(new ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6));
		}
		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return _store.DispatchThunk(new ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
		{
			return _store.DispatchThunk(new ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
		}
	}
}
