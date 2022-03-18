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

	public Task<SendResult> SendIdCryptInvitationToRtgsAsync(IdCryptInvitationV1 message, CancellationToken cancellationToken) =>
		_internalPublisher.SendMessageAsync(message, "idcrypt.invitation.tortgs.v1", cancellationToken);

	public Task<SendResult> SendIdCryptInvitationToBankAsync(
		IdCryptInvitationV1 message,
		string bankPartnerDid,
		CancellationToken cancellationToken)
	{
		var headers = new Dictionary<string, string> { { "bankpartnerdid", bankPartnerDid } };
		return _internalPublisher.SendMessageAsync(message, "idcrypt.invitation.tobank.v1", cancellationToken, headers);
	}
}
