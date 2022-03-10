using RTGS.DotNetSDK.Subscriber;
using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK;

/// <summary>
/// The IRtgsSubscriber interface, implementations of this interface are responsible for subscribing to messages from the RTGS platform
/// </summary>
public interface IRtgsSubscriber : IAsyncDisposable
{
	/// <summary>
	/// Gets whether this instance is running and will receive messages.
	/// </summary>
	bool IsRunning { get; }

	/// <summary>
	/// Event that occurs when an exception occurs.
	/// </summary>
	event EventHandler<ExceptionEventArgs> OnExceptionOccurred;

	/// <summary>
	/// Starts the subscriber so it can receive messages from the RTGS platform.
	/// </summary>
	/// <param name="handlers">Collection of <see cref="IHandler"/> instances that will be used to handle received messages.</param>
	/// <returns>A task that will complete when the subscriber has been started.</returns>
	Task StartAsync(IEnumerable<IHandler> handlers);

	/// <summary>
	/// Stops the subscriber so will no longer receive messages from the RTGS platform.
	/// </summary>
	/// <returns>A task that will complete when the subscriber has been stopped.</returns>
	Task StopAsync();
}
