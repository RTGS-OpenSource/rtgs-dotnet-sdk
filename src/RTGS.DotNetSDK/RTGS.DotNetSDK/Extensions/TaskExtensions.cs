using Microsoft.Extensions.Logging;

namespace RTGS.DotNetSDK.Extensions;

public static class TaskExtensions
{
	// https://www.meziantou.net/fire-and-forget-a-task-in-dotnet.htm
	public static void Forget(this Task task, Action<Exception> logErrorAction)
	{
		if (!task.IsCompleted || task.IsFaulted)
		{
			_ = ForgetAwaited(task, logErrorAction);
		}

		static async Task ForgetAwaited(Task task, Action<Exception> logErrorAction)
		{
			try
			{
				await task.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				logErrorAction(ex);
			}
		}
	}
}
