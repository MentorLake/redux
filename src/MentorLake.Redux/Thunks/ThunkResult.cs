using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace MentorLake.Redux.Thunks;

public class ThunkResult
{
	public IObservable<object> Actions { get; init; }

	public Task ToTask()
	{
		return Actions
			.Do(a =>
			{
				if (a is ThunkRejected rejected) ExceptionDispatchInfo.Throw(rejected.Exception);
			})
			.OfType<ThunkFulfilled>()
			.Take(1)
			.ToTask();
	}
}

public class ThunkResult<TResult> : ThunkResult
{
	public new Task<TResult> ToTask()
	{
		return Actions
			.Do(a =>
			{
				if (a is ThunkRejected rejected) ExceptionDispatchInfo.Throw(rejected.Exception);
			})
			.OfType<ThunkFulfilled<TResult>>()
			.Select(a => a.Result)
			.Take(1)
			.ToTask();
	}
}
