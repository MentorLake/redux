namespace MentorLake.Redux.Thunks;

public sealed class ThunkGroup(string name)
{
	private CancellationTokenSource _current;

	public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

	public CancellationTokenSource Begin(CancellationToken callerToken)
	{
		if (_current != null)
		{
			_current.CancelQuietly();
		}

		var cancellation = callerToken.CanBeCanceled
			? CancellationTokenSource.CreateLinkedTokenSource(callerToken)
			: new();

		_current = cancellation;
		return cancellation;
	}

	public void End(CancellationTokenSource cancellation)
	{
		if (ReferenceEquals(_current, cancellation))
		{
			_current = null;
		}

		cancellation.Dispose();
	}
}
