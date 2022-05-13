using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Payment.V4;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

/// <summary>
/// Interface to define a message adapter.
/// </summary>
public interface IMessageAdapter
{
	/// <summary>
	/// Gets the identifier of messages that can be adapted.
	/// </summary>
	string MessageIdentifier { get; }
}

/// <summary>
/// Interface to define an adaptor for messages of type <see cref="TMessage"/>.
/// </summary>
/// <typeparam name="TMessage">The type of message that will be adapted.</typeparam>
public interface IMessageAdapter<out TMessage> : IMessageAdapter
{
	/// <summary>
	/// Adapts the <see cref="RtgsMessage"/> before passing to the supplied handler.
	/// </summary>
	/// <param name="rtgsMessage">The <see cref="RtgsMessage"/> to be adapted.</param>
	/// <param name="handler">The handler to pass the message to.</param>
	/// <returns>A task that will complete when the message has been handled.</returns>
	Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<TMessage> handler);
}
