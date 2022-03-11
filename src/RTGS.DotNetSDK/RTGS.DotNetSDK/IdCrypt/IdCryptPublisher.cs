using RTGS.DotNetSDK.Publisher;
using RTGS.DotNetSDK.Publisher.Messages;

namespace RTGS.DotNetSDK.IdCrypt;

internal class IdCryptPublisher : IIdCryptPublisher
{
	private readonly IInternalPublisher _internalPublisher;

	public IdCryptPublisher(IInternalPublisher internalPublisher)
	{
		_internalPublisher = internalPublisher;
	}

	public Task<SendResult> SendIdCryptInvitationAsync(IdCryptInvitationV1 message, CancellationToken cancellationToken) =>
		_internalPublisher.SendMessageAsync(message, "idcrypt.invitation.v1", cancellationToken);
}
