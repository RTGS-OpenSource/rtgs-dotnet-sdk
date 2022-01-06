using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.HandleMessageCommands;

/// <summary>
/// Interface to define a handle message command.
/// </summary>
public interface IHandleMessageCommand
{
	/// <summary>
	/// Gets the identifier of the messages that can be handled.
	/// </summary>
	string MessageIdentifier { get; }

	/// <summary>
	/// Handles the supplied <see cref="RtgsMessage"/>.
	/// </summary>
	/// <param name="rtgsMessage">The <see cref="RtgsMessage"/> to handle.</param>
	/// <returns>A task that will complete when the message has been handled.</returns>
	Task HandleAsync(RtgsMessage rtgsMessage);
}
