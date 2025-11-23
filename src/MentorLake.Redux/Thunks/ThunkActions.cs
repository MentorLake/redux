namespace MentorLake.Redux.Thunks;

public class ThunkPending(string actionName) : IAction
{
	public string ActionName => $"{actionName}/pending";
}

public class ThunkRejected(string actionName, Exception exception)  : IAction
{
	public Exception Exception => exception;
	public string ActionName => $"{actionName}/rejected";
}

public class ThunkFulfilled<TResult>(string actionName, TResult result) : ThunkFulfilled(actionName)
{
	public TResult Result => result;
}

public class ThunkFulfilled(string actionName) : IAction
{
	public string ActionName => $"{actionName}/fulfilled";
}
