using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.Subscriber;

public interface IRtgsSubscriber : IAsyncDisposable
{
	bool IsRunning { get; }

	event EventHandler<ExceptionEventArgs> OnExceptionOccurred;

	Task StartAsync(IEnumerable<IHandler> handlers);

	Task StopAsync();
}
