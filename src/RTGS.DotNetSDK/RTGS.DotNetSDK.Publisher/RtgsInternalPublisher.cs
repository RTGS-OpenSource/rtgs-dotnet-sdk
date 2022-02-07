using RTGS.DotNetSDK.Publisher.Messages;

namespace RTGS.DotNetSDK.Publisher;

internal class RtgsInternalPublisher : IRtgsInternalPublisher
{
	private readonly IMessagePublisher _messagePublisher;

	public RtgsInternalPublisher(IMessagePublisher messagePublisher)
	{
		_messagePublisher = messagePublisher;
	}

	public Task<SendResult> SendIdCryptInvitationAsync(IdCryptInvitationV1 message, CancellationToken cancellationToken) =>
		_messagePublisher.SendMessage(message, "idcrypt.invitation.v1", cancellationToken);
}
