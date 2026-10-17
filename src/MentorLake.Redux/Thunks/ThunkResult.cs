using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace MentorLake.Redux.Thunks;

public class ThunkResult
{
	private readonly CancellationTokenSource _cancellation;

	internal ThunkResult(IObservable<object> actions, CancellationTokenSource cancellation)
	{
		Actions = actions;
		_cancellation = cancellation;
	}

	public IObservable<object> Actions { get; init; }

	public void Cancel()
	{
		if (_cancellation == null) return;
		try
		{
			_cancellation.Cancel();
		}
		catch (ObjectDisposedException)
		{
		}
	}

	public Task ToTask()
	{
		return Actions
			.Do(a =>
			{
				if (a is ThunkRejected rejected) ExceptionDispatchInfo.Throw(rejected.Exception);
				if (a is ThunkCancelled) throw new OperationCanceledException();
			})
			.OfType<ThunkFulfilled>()
			.Take(1)
			.ToTask();
	}
}

public class ThunkResult<TResult> : ThunkResult
{
	internal ThunkResult(IObservable<object> actions, CancellationTokenSource cancellation)
		: base(actions, cancellation)
	{
	}

	public new Task<TResult> ToTask()
	{
		return Actions
			.Do(a =>
			{
				if (a is ThunkRejected rejected) ExceptionDispatchInfo.Throw(rejected.Exception);
				if (a is ThunkCancelled) throw new OperationCanceledException();
			})
			.OfType<ThunkFulfilled<TResult>>()
			.Select(a => a.Result)
			.Take(1)
			.ToTask();
	}
}
