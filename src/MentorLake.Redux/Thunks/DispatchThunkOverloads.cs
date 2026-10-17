
using MentorLake.Redux.Thunks;

namespace MentorLake.Redux
{
	public sealed partial class ReduxStore
	{
		public ThunkResult DispatchThunk(ThunkAction<ThunkApi> thunk, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(), cancellationToken);
		}

		public ThunkResult DispatchThunk(Func<ThunkApi, Task> thunk, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkAction<ThunkApi>(string.Empty, thunk).Bind(), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TResult>(ThunkFunc<ThunkApi, TResult> thunk, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TResult>(Func<ThunkApi, Task<TResult>> thunk, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TResult>(string.Empty, thunk).Bind(), cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1>(Func<ThunkApi, TArg1, Task> work, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkAction<ThunkApi, TArg1>(string.Empty, work).Bind(arg1), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TResult>(Func<ThunkApi, TArg1, Task<TResult>> work, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TArg1, TResult>(string.Empty, work).Bind(arg1), cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1>(ThunkAction<ThunkApi, TArg1> thunk, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(arg1), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TResult>(ThunkFunc<ThunkApi, TArg1, TResult> thunk, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(arg1), cancellationToken);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2>(Func<ThunkApi, TArg1, TArg2, Task> work, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkAction<ThunkApi, TArg1, TArg2>(string.Empty, work).Bind(arg1, arg2), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TResult>(Func<ThunkApi, TArg1, TArg2, Task<TResult>> work, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TArg1, TArg2, TResult>(string.Empty, work).Bind(arg1, arg2), cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2>(ThunkAction<ThunkApi, TArg1, TArg2> thunk, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TResult> thunk, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2), cancellationToken);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3>(Func<ThunkApi, TArg1, TArg2, TArg3, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkAction<ThunkApi, TArg1, TArg2, TArg3>(string.Empty, work).Bind(arg1, arg2, arg3), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TResult>(string.Empty, work).Bind(arg1, arg2, arg3), cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3), cancellationToken);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4>(string.Empty, work).Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TResult>(string.Empty, work).Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(new ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}
		public ThunkResult DispatchThunkLatest(string cancellationGroup, ThunkAction<ThunkApi> thunk, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(), cancellationToken);
		}

		public ThunkResult DispatchThunkLatest(string cancellationGroup, Func<ThunkApi, Task> work, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkAction<ThunkApi>(string.Empty, work).Bind(), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TResult>(string cancellationGroup, ThunkFunc<ThunkApi, TResult> thunk, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TResult>(string cancellationGroup, Func<ThunkApi, Task<TResult>> work, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkFunc<ThunkApi, TResult>(string.Empty, work).Bind(), cancellationToken);
		}
		public ThunkResult DispatchThunkLatest<TArg1>(string cancellationGroup, Func<ThunkApi, TArg1, Task> work, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkAction<ThunkApi, TArg1>(string.Empty, work).Bind(arg1), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TArg1, TResult>(string cancellationGroup, Func<ThunkApi, TArg1, Task<TResult>> work, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkFunc<ThunkApi, TArg1, TResult>(string.Empty, work).Bind(arg1), cancellationToken);
		}

		public ThunkResult DispatchThunkLatest<TArg1>(string cancellationGroup, ThunkAction<ThunkApi, TArg1> thunk, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TArg1, TResult>(string cancellationGroup, ThunkFunc<ThunkApi, TArg1, TResult> thunk, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1), cancellationToken);
		}
		public ThunkResult DispatchThunkLatest<TArg1, TArg2>(string cancellationGroup, Func<ThunkApi, TArg1, TArg2, Task> work, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkAction<ThunkApi, TArg1, TArg2>(string.Empty, work).Bind(arg1, arg2), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TArg1, TArg2, TResult>(string cancellationGroup, Func<ThunkApi, TArg1, TArg2, Task<TResult>> work, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkFunc<ThunkApi, TArg1, TArg2, TResult>(string.Empty, work).Bind(arg1, arg2), cancellationToken);
		}

		public ThunkResult DispatchThunkLatest<TArg1, TArg2>(string cancellationGroup, ThunkAction<ThunkApi, TArg1, TArg2> thunk, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TArg1, TArg2, TResult>(string cancellationGroup, ThunkFunc<ThunkApi, TArg1, TArg2, TResult> thunk, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2), cancellationToken);
		}
		public ThunkResult DispatchThunkLatest<TArg1, TArg2, TArg3>(string cancellationGroup, Func<ThunkApi, TArg1, TArg2, TArg3, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkAction<ThunkApi, TArg1, TArg2, TArg3>(string.Empty, work).Bind(arg1, arg2, arg3), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TArg1, TArg2, TArg3, TResult>(string cancellationGroup, Func<ThunkApi, TArg1, TArg2, TArg3, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TResult>(string.Empty, work).Bind(arg1, arg2, arg3), cancellationToken);
		}

		public ThunkResult DispatchThunkLatest<TArg1, TArg2, TArg3>(string cancellationGroup, ThunkAction<ThunkApi, TArg1, TArg2, TArg3> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TArg1, TArg2, TArg3, TResult>(string cancellationGroup, ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3), cancellationToken);
		}
		public ThunkResult DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4>(string cancellationGroup, Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4>(string.Empty, work).Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4, TResult>(string cancellationGroup, Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TResult>(string.Empty, work).Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}

		public ThunkResult DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4>(string cancellationGroup, ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4, TResult>(string cancellationGroup, ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}
		public ThunkResult DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4, TArg5>(string cancellationGroup, Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string cancellationGroup, Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}

		public ThunkResult DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4, TArg5>(string cancellationGroup, ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string cancellationGroup, ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}
		public ThunkResult DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(string cancellationGroup, Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(string cancellationGroup, Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}

		public ThunkResult DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(string cancellationGroup, ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(string cancellationGroup, ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}
		public ThunkResult DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(string cancellationGroup, Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(string cancellationGroup, Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, new ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(string.Empty, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}

		public ThunkResult DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(string cancellationGroup, ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunkLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(string cancellationGroup, ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}
	}
}

namespace MentorLake.Redux.Thunks
{
	public partial class ThunkApi
	{
		public ThunkResult DispatchThunk<TApi>(ICallableThunkAction<TApi> thunk, CancellationToken cancellationToken = default) where TApi : ThunkApi
		{
			return store.DispatchThunk(thunk, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TApi, TResult>(ICallableThunkFunc<TApi, TResult> thunk, CancellationToken cancellationToken = default) where TApi : ThunkApi
		{
			return store.DispatchThunk(thunk, cancellationToken);
		}

		public ThunkResult DispatchThunk(ThunkAction<ThunkApi> thunk, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, cancellationToken);
		}

		public ThunkResult DispatchThunk(Func<ThunkApi, Task> thunk, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(new ThunkAction<ThunkApi>(string.Empty, thunk), cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TResult>(ThunkFunc<ThunkApi, TResult> thunk, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TResult>(Func<ThunkApi, Task<TResult>> thunk, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(new ThunkFunc<ThunkApi, TResult>(string.Empty, thunk), cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1>(Func<ThunkApi, TArg1, Task> work, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TResult>(Func<ThunkApi, TArg1, Task<TResult>> work, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1>(ThunkAction<ThunkApi, TArg1> thunk, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TResult>(ThunkFunc<ThunkApi, TArg1, TResult> thunk, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, cancellationToken);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2>(Func<ThunkApi, TArg1, TArg2, Task> work, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TResult>(Func<ThunkApi, TArg1, TArg2, Task<TResult>> work, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2>(ThunkAction<ThunkApi, TArg1, TArg2> thunk, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TResult> thunk, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, cancellationToken);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3>(Func<ThunkApi, TArg1, TArg2, TArg3, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, cancellationToken);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4, cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4, cancellationToken);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4, arg5, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4, arg5, cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4, arg5, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4, arg5, cancellationToken);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4, arg5, arg6, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4, arg5, arg6, cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4, arg5, arg6, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4, arg5, arg6, cancellationToken);
		}
		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4, arg5, arg6, arg7, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(Func<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(work, arg1, arg2, arg3, arg4, arg5, arg6, arg7, cancellationToken);
		}

		public ThunkResult DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(ThunkAction<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4, arg5, arg6, arg7, cancellationToken);
		}

		public ThunkResult<TResult> DispatchThunk<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(ThunkFunc<ThunkApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return Using<ThunkApi>().Dispatch(thunk, arg1, arg2, arg3, arg4, arg5, arg6, arg7, cancellationToken);
		}
	}

	public sealed partial class ThunkDispatcher<TApi> where TApi : ThunkApi
	{
		public ThunkResult Dispatch(CallableThunkAction<TApi> thunk, CancellationToken cancellationToken = default) => _store.DispatchThunk(thunk, cancellationToken);
		public ThunkResult Dispatch(ThunkAction<TApi> thunk, CancellationToken cancellationToken = default) => Dispatch(thunk.Bind(), cancellationToken);
		public ThunkResult Dispatch(string actionName, Func<TApi, Task> work, CancellationToken cancellationToken = default) => Dispatch(new ThunkAction<TApi>(actionName ?? string.Empty, work).Bind(), cancellationToken);
		public ThunkResult Dispatch(Func<TApi, Task> work, CancellationToken cancellationToken = default) => Dispatch(string.Empty, work, cancellationToken);

		public ThunkResult<TResult> Dispatch<TResult>(CallableThunkFunc<TApi, TResult> thunk, CancellationToken cancellationToken = default) => _store.DispatchThunk(thunk, cancellationToken);
		public ThunkResult<TResult> Dispatch<TResult>(ThunkFunc<TApi, TResult> thunk, CancellationToken cancellationToken = default) => Dispatch(thunk.Bind(), cancellationToken);
		public ThunkResult<TResult> Dispatch<TResult>(string actionName, Func<TApi, Task<TResult>> work, CancellationToken cancellationToken = default) => Dispatch(new ThunkFunc<TApi, TResult>(actionName ?? string.Empty, work), cancellationToken);
		public ThunkResult<TResult> Dispatch<TResult>(Func<TApi, Task<TResult>> work, CancellationToken cancellationToken = default) => Dispatch(string.Empty, work, cancellationToken);
		public ThunkResult Dispatch<TArg1>(ThunkAction<TApi, TArg1> thunk, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(thunk.Bind(arg1), cancellationToken);
		}

		public ThunkResult Dispatch<TArg1>(Func<TApi, TArg1, Task> work, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return Dispatch(string.Empty, work, arg1, cancellationToken);
		}

		public ThunkResult Dispatch<TArg1>(string actionName, Func<TApi, TArg1, Task> work, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(new ThunkAction<TApi, TArg1>(actionName, work).Bind(arg1), cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TResult>(ThunkFunc<TApi, TArg1, TResult> thunk, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(thunk.Bind(arg1), cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TResult>(Func<TApi, TArg1, Task<TResult>> work, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return Dispatch(string.Empty, work, arg1, cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TResult>(string actionName, Func<TApi, TArg1, Task<TResult>> work, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(new ThunkFunc<TApi, TArg1, TResult>(actionName, work).Bind(arg1), cancellationToken);
		}
		public ThunkResult Dispatch<TArg1, TArg2>(ThunkAction<TApi, TArg1, TArg2> thunk, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2), cancellationToken);
		}

		public ThunkResult Dispatch<TArg1, TArg2>(Func<TApi, TArg1, TArg2, Task> work, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return Dispatch(string.Empty, work, arg1, arg2, cancellationToken);
		}

		public ThunkResult Dispatch<TArg1, TArg2>(string actionName, Func<TApi, TArg1, TArg2, Task> work, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(new ThunkAction<TApi, TArg1, TArg2>(actionName, work).Bind(arg1, arg2), cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TResult>(ThunkFunc<TApi, TArg1, TArg2, TResult> thunk, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2), cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TResult>(Func<TApi, TArg1, TArg2, Task<TResult>> work, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return Dispatch(string.Empty, work, arg1, arg2, cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TResult>(string actionName, Func<TApi, TArg1, TArg2, Task<TResult>> work, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(new ThunkFunc<TApi, TArg1, TArg2, TResult>(actionName, work).Bind(arg1, arg2), cancellationToken);
		}
		public ThunkResult Dispatch<TArg1, TArg2, TArg3>(ThunkAction<TApi, TArg1, TArg2, TArg3> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3), cancellationToken);
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3>(Func<TApi, TArg1, TArg2, TArg3, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, cancellationToken);
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3>(string actionName, Func<TApi, TArg1, TArg2, TArg3, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(new ThunkAction<TApi, TArg1, TArg2, TArg3>(actionName, work).Bind(arg1, arg2, arg3), cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TResult>(ThunkFunc<TApi, TArg1, TArg2, TArg3, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3), cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TResult>(Func<TApi, TArg1, TArg2, TArg3, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TResult>(string actionName, Func<TApi, TArg1, TArg2, TArg3, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(new ThunkFunc<TApi, TArg1, TArg2, TArg3, TResult>(actionName, work).Bind(arg1, arg2, arg3), cancellationToken);
		}
		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4>(ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4>(Func<TApi, TArg1, TArg2, TArg3, TArg4, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4, cancellationToken);
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(new ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4>(actionName, work).Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TResult>(ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TResult>(Func<TApi, TArg1, TArg2, TArg3, TArg4, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4, cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TResult>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(new ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TResult>(actionName, work).Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}
		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5>(ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5>(Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4, arg5, cancellationToken);
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(new ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4, arg5, cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(new ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}
		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6, cancellationToken);
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(new ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6, cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(new ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}
		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6, arg7, cancellationToken);
		}

		public ThunkResult Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(new ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return Dispatch(string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6, arg7, cancellationToken);
		}

		public ThunkResult<TResult> Dispatch<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunk(new ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}

		public ThunkResult DispatchLatest(string cancellationGroup, CallableThunkAction<TApi> thunk, CancellationToken cancellationToken = default) => _store.DispatchThunkLatest(cancellationGroup, thunk, cancellationToken);
		public ThunkResult DispatchLatest(string cancellationGroup, ThunkAction<TApi> thunk, CancellationToken cancellationToken = default) => DispatchLatest(cancellationGroup, thunk.Bind(), cancellationToken);
		public ThunkResult DispatchLatest(string cancellationGroup, string actionName, Func<TApi, Task> work, CancellationToken cancellationToken = default) => DispatchLatest(cancellationGroup, new ThunkAction<TApi>(actionName ?? string.Empty, work).Bind(), cancellationToken);
		public ThunkResult DispatchLatest(string cancellationGroup, Func<TApi, Task> work, CancellationToken cancellationToken = default) => DispatchLatest(cancellationGroup, string.Empty, work, cancellationToken);

		public ThunkResult<TResult> DispatchLatest<TResult>(string cancellationGroup, CallableThunkFunc<TApi, TResult> thunk, CancellationToken cancellationToken = default) => _store.DispatchThunkLatest(cancellationGroup, thunk, cancellationToken);
		public ThunkResult<TResult> DispatchLatest<TResult>(string cancellationGroup, ThunkFunc<TApi, TResult> thunk, CancellationToken cancellationToken = default) => DispatchLatest(cancellationGroup, thunk.Bind(), cancellationToken);
		public ThunkResult<TResult> DispatchLatest<TResult>(string cancellationGroup, string actionName, Func<TApi, Task<TResult>> work, CancellationToken cancellationToken = default) => DispatchLatest(cancellationGroup, new ThunkFunc<TApi, TResult>(actionName ?? string.Empty, work), cancellationToken);
		public ThunkResult<TResult> DispatchLatest<TResult>(string cancellationGroup, Func<TApi, Task<TResult>> work, CancellationToken cancellationToken = default) => DispatchLatest(cancellationGroup, string.Empty, work, cancellationToken);
		public ThunkResult DispatchLatest<TArg1>(string cancellationGroup, ThunkAction<TApi, TArg1> thunk, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1), cancellationToken);
		}

		public ThunkResult DispatchLatest<TArg1>(string cancellationGroup, Func<TApi, TArg1, Task> work, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return DispatchLatest(cancellationGroup, string.Empty, work, arg1, cancellationToken);
		}

		public ThunkResult DispatchLatest<TArg1>(string cancellationGroup, string actionName, Func<TApi, TArg1, Task> work, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, new ThunkAction<TApi, TArg1>(actionName, work).Bind(arg1), cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TResult>(string cancellationGroup, ThunkFunc<TApi, TArg1, TResult> thunk, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1), cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TResult>(string cancellationGroup, Func<TApi, TArg1, Task<TResult>> work, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return DispatchLatest(cancellationGroup, string.Empty, work, arg1, cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TResult>(string cancellationGroup, string actionName, Func<TApi, TArg1, Task<TResult>> work, TArg1 arg1, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, new ThunkFunc<TApi, TArg1, TResult>(actionName, work).Bind(arg1), cancellationToken);
		}
		public ThunkResult DispatchLatest<TArg1, TArg2>(string cancellationGroup, ThunkAction<TApi, TArg1, TArg2> thunk, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2), cancellationToken);
		}

		public ThunkResult DispatchLatest<TArg1, TArg2>(string cancellationGroup, Func<TApi, TArg1, TArg2, Task> work, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return DispatchLatest(cancellationGroup, string.Empty, work, arg1, arg2, cancellationToken);
		}

		public ThunkResult DispatchLatest<TArg1, TArg2>(string cancellationGroup, string actionName, Func<TApi, TArg1, TArg2, Task> work, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, new ThunkAction<TApi, TArg1, TArg2>(actionName, work).Bind(arg1, arg2), cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TResult>(string cancellationGroup, ThunkFunc<TApi, TArg1, TArg2, TResult> thunk, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2), cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TResult>(string cancellationGroup, Func<TApi, TArg1, TArg2, Task<TResult>> work, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return DispatchLatest(cancellationGroup, string.Empty, work, arg1, arg2, cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TResult>(string cancellationGroup, string actionName, Func<TApi, TArg1, TArg2, Task<TResult>> work, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, new ThunkFunc<TApi, TArg1, TArg2, TResult>(actionName, work).Bind(arg1, arg2), cancellationToken);
		}
		public ThunkResult DispatchLatest<TArg1, TArg2, TArg3>(string cancellationGroup, ThunkAction<TApi, TArg1, TArg2, TArg3> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3), cancellationToken);
		}

		public ThunkResult DispatchLatest<TArg1, TArg2, TArg3>(string cancellationGroup, Func<TApi, TArg1, TArg2, TArg3, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return DispatchLatest(cancellationGroup, string.Empty, work, arg1, arg2, arg3, cancellationToken);
		}

		public ThunkResult DispatchLatest<TArg1, TArg2, TArg3>(string cancellationGroup, string actionName, Func<TApi, TArg1, TArg2, TArg3, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, new ThunkAction<TApi, TArg1, TArg2, TArg3>(actionName, work).Bind(arg1, arg2, arg3), cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TArg3, TResult>(string cancellationGroup, ThunkFunc<TApi, TArg1, TArg2, TArg3, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3), cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TArg3, TResult>(string cancellationGroup, Func<TApi, TArg1, TArg2, TArg3, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return DispatchLatest(cancellationGroup, string.Empty, work, arg1, arg2, arg3, cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TArg3, TResult>(string cancellationGroup, string actionName, Func<TApi, TArg1, TArg2, TArg3, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, new ThunkFunc<TApi, TArg1, TArg2, TArg3, TResult>(actionName, work).Bind(arg1, arg2, arg3), cancellationToken);
		}
		public ThunkResult DispatchLatest<TArg1, TArg2, TArg3, TArg4>(string cancellationGroup, ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}

		public ThunkResult DispatchLatest<TArg1, TArg2, TArg3, TArg4>(string cancellationGroup, Func<TApi, TArg1, TArg2, TArg3, TArg4, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return DispatchLatest(cancellationGroup, string.Empty, work, arg1, arg2, arg3, arg4, cancellationToken);
		}

		public ThunkResult DispatchLatest<TArg1, TArg2, TArg3, TArg4>(string cancellationGroup, string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, new ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4>(actionName, work).Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TArg3, TArg4, TResult>(string cancellationGroup, ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TArg3, TArg4, TResult>(string cancellationGroup, Func<TApi, TArg1, TArg2, TArg3, TArg4, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return DispatchLatest(cancellationGroup, string.Empty, work, arg1, arg2, arg3, arg4, cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TArg3, TArg4, TResult>(string cancellationGroup, string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, new ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TResult>(actionName, work).Bind(arg1, arg2, arg3, arg4), cancellationToken);
		}
		public ThunkResult DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5>(string cancellationGroup, ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}

		public ThunkResult DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5>(string cancellationGroup, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return DispatchLatest(cancellationGroup, string.Empty, work, arg1, arg2, arg3, arg4, arg5, cancellationToken);
		}

		public ThunkResult DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5>(string cancellationGroup, string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, new ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string cancellationGroup, ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string cancellationGroup, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return DispatchLatest(cancellationGroup, string.Empty, work, arg1, arg2, arg3, arg4, arg5, cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string cancellationGroup, string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, new ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5), cancellationToken);
		}
		public ThunkResult DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(string cancellationGroup, ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}

		public ThunkResult DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(string cancellationGroup, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return DispatchLatest(cancellationGroup, string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6, cancellationToken);
		}

		public ThunkResult DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(string cancellationGroup, string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, new ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(string cancellationGroup, ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(string cancellationGroup, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return DispatchLatest(cancellationGroup, string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6, cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(string cancellationGroup, string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, new ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6), cancellationToken);
		}
		public ThunkResult DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(string cancellationGroup, ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}

		public ThunkResult DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(string cancellationGroup, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return DispatchLatest(cancellationGroup, string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6, arg7, cancellationToken);
		}

		public ThunkResult DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(string cancellationGroup, string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, new ThunkAction<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(string cancellationGroup, ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> thunk, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, thunk.Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(string cancellationGroup, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return DispatchLatest(cancellationGroup, string.Empty, work, arg1, arg2, arg3, arg4, arg5, arg6, arg7, cancellationToken);
		}

		public ThunkResult<TResult> DispatchLatest<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(string cancellationGroup, string actionName, Func<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, Task<TResult>> work, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, CancellationToken cancellationToken = default)
		{
			return _store.DispatchThunkLatest(cancellationGroup, new ThunkFunc<TApi, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(actionName, work).Bind(arg1, arg2, arg3, arg4, arg5, arg6, arg7), cancellationToken);
		}
	}
}
