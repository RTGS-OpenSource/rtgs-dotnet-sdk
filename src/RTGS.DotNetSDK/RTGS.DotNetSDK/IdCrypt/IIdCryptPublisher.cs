using RTGS.DotNetSDK.Publisher.Messages;

namespace RTGS.DotNetSDK.IdCrypt;

internal interface IIdCryptPublisher
{
	Task<SendResult> SendIdCryptInvitationToRtgsAsync(IdCryptInvitationV1 message, CancellationToken cancellationToken);
	Task<SendResult> SendIdCryptInvitationToBankAsync(IdCryptInvitationV1 message, string bankPartnerDid, CancellationToken cancellationToken);
}
