namespace MentorLake.Redux;

public static class CancellationTokenSourceExtensions
{
	public static void CancelQuietly(this CancellationTokenSource cancellation)
	{
		try
		{
			cancellation.Cancel();
		}
		catch (ObjectDisposedException)
		{
		}
	}
}
