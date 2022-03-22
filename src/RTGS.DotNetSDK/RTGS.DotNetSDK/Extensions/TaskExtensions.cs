using Microsoft.Extensions.Logging;

namespace RTGS.DotNetSDK.Extensions;

public static class TaskExtensions
{
	// https://www.meziantou.net/fire-and-forget-a-task-in-dotnet.htm
	public static void Forget(this Task task, ILogger logger)
	{
		if (!task.IsCompleted || task.IsFaulted)
		{
			_ = ForgetAwaited(task, logger);
		}

		static async Task ForgetAwaited(Task task, ILogger logger)
		{
			try
			{
				await task.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Big bada boom!");
			}
		}
	}
}
