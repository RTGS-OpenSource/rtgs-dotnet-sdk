using RTGS.DotNetSDK.Publisher.Messages;

namespace RTGS.DotNetSDK.IdCrypt;

internal interface IIdCryptPublisher
{
	Task<SendResult> SendIdCryptInvitationAsync(IdCryptInvitationV1 message, CancellationToken cancellationToken);
}
