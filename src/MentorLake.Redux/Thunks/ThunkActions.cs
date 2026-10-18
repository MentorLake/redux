namespace MentorLake.Redux.Thunks;

/// <summary>Lifecycle action dispatched when a thunk starts.</summary>
public class ThunkPending(string actionName) : IAction
{
	public string ActionName => $"{actionName}/pending";
}

/// <summary>Lifecycle action dispatched when a thunk throws (cancellation excluded).</summary>
public class ThunkRejected(string actionName, Exception exception)  : IAction
{
	/// <summary>The exception the thunk threw.</summary>
	public Exception Exception => exception;
	public string ActionName => $"{actionName}/rejected";
}

/// <summary>Lifecycle action dispatched when a thunk's work was cancelled.</summary>
public class ThunkCancelled(string actionName) : IAction
{
	public string ActionName => $"{actionName}/cancelled";
}

/// <summary>Lifecycle action carrying the thunk's return value.</summary>
public class ThunkFulfilled<TResult>(string actionName, TResult result) : ThunkFulfilled(actionName)
{
	/// <summary>The value the thunk function returned.</summary>
	public TResult Result => result;
}

/// <summary>Lifecycle action dispatched when a thunk action completes successfully.</summary>
public class ThunkFulfilled(string actionName) : IAction
{
	public string ActionName => $"{actionName}/fulfilled";
}
