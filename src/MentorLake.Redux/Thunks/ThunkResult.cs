using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace MentorLake.Redux.Thunks;

/// <summary>
/// Handle for a dispatched thunk run. Exposes the run's lifecycle action stream, awaiting
/// via <c>ToTask()</c>, and cancellation. The thunk starts when the result is first
/// subscribed or awaited — not at dispatch time.
/// </summary>
public class ThunkResult
{
	private readonly CancellationTokenSource _cancellation;

	internal ThunkResult(IObservable<object> actions, CancellationTokenSource cancellation)
	{
		Actions = actions;
		_cancellation = cancellation;
	}

	/// <summary>
	/// This run's lifecycle actions (<see cref="ThunkPending"/>,
	/// <see cref="ThunkFulfilled"/>, <see cref="ThunkRejected"/>,
	/// <see cref="ThunkCancelled"/>). Subscribing starts the run.
	/// </summary>
	public IObservable<object> Actions { get; init; }

	/// <summary>
	/// Cancels the run. The thunk observes cancellation through
	/// <see cref="ThunkApi.CancellationToken"/> and emits <see cref="ThunkCancelled"/>.
	/// </summary>
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

	/// <summary>
	/// Awaits completion: resolves on <see cref="ThunkFulfilled"/>, rethrows the thunk's
	/// exception on <see cref="ThunkRejected"/>, and throws
	/// <see cref="OperationCanceledException"/> on <see cref="ThunkCancelled"/>.
	/// </summary>
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

/// <summary>
/// Handle for a dispatched thunk function run; adds the typed result to
/// <see cref="ThunkResult"/>.
/// </summary>
/// <typeparam name="TResult">The thunk function's return type.</typeparam>
public class ThunkResult<TResult> : ThunkResult
{
	internal ThunkResult(IObservable<object> actions, CancellationTokenSource cancellation)
		: base(actions, cancellation)
	{
	}

	/// <summary>
	/// Awaits completion and yields the thunk function's return value. Failure semantics
	/// match <see cref="ThunkResult.ToTask"/>.
	/// </summary>
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
