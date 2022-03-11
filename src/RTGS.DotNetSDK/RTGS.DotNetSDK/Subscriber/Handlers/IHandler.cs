namespace RTGS.DotNetSDK.Subscriber.Handlers;

/// <summary>
/// Interface to define a message handler.
/// </summary>
public interface IHandler { }

/// <summary>
/// Interface to define a handler for messages of type <see cref="TMessage"/>.
/// </summary>
/// <typeparam name="TMessage">The type of message that will be handled.</typeparam>
public interface IHandler<in TMessage> : IHandler
{
	/// <summary>
	/// Handles the supplied message.
	/// </summary>
	/// <param name="message">The message to handle.</param>
	/// <returns>A task that will complete when the message has been handled.</returns>
	/// <remarks>
	/// The message should be handled as fast as possible.
	/// Any long running operations should be run out of process.
	/// </remarks>
	Task HandleMessageAsync(TMessage message);
}
