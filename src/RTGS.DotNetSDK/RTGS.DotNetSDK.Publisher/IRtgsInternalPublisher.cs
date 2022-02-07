using RTGS.DotNetSDK.Publisher.Messages;

namespace RTGS.DotNetSDK.Publisher;

internal interface IRtgsInternalPublisher 
{
	Task<SendResult> SendIdCryptInvitationAsync(IdCryptInvitationV1 message, CancellationToken cancellationToken);
}
