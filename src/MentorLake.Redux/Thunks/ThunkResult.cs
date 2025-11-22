using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace MentorLake.Redux.Thunks;

public class ThunkResult<TActions, TResult>
{
	public IObservable<object> Actions { get; init; }

	public Task<TResult> ToTask()
	{
		return Actions
			.Do(a =>
			{
				if (a is ThunkRejected<TActions> rejected) ExceptionDispatchInfo.Throw(rejected.Exception);
			})
			.OfType<ThunkFulfilled<TActions, TResult>>()
			.Select(a => a.Result)
			.Take(1)
			.ToTask();
	}
}
