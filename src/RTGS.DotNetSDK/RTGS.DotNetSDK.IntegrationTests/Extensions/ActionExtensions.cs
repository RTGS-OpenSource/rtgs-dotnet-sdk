namespace RTGS.DotNetSDK.IntegrationTests.Extensions;

public static class ActionExtensions
{
	public static void Within(this Action assertion, int timeoutMilliseconds)
	{
		var spinWait = new SpinWait();

		var startTime = Environment.TickCount;
		while (true)
		{
			Exception exception;
			try
			{
				assertion();
				return;
			}
			catch (Exception ex)
			{
				exception = ex;
			}

			if (timeoutMilliseconds <= Environment.TickCount - startTime)
			{
				throw exception;
			}

			spinWait.SpinOnce();
		}
	}
}
