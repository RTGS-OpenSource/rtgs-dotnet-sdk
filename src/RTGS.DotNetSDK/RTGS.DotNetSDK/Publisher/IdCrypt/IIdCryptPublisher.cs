using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;

namespace RTGS.DotNetSDK.Publisher.IdCrypt;

internal interface IIdCryptPublisher
{
	Task<SendResult> SendIdCryptInvitationToRtgsAsync(IdCryptInvitationV1 message, CancellationToken cancellationToken);
	Task<SendResult> SendIdCryptInvitationToBankAsync(IdCryptInvitationV1 message, string bankPartnerRtgsGlobalId, CancellationToken cancellationToken);
	Task<SendResult> SendIdCryptInvitationConfirmationAsync(IdCryptInvitationConfirmationV1 message, string bankPartnerRtgsGlobalId, CancellationToken cancellationToken);
}
