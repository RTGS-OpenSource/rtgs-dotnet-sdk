using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Publisher;

internal class RtgsInternalPublisher : RtgsPublisherBase, IRtgsInternalPublisher
{
	public RtgsInternalPublisher(ILogger<RtgsPublisher> logger, Payment.PaymentClient paymentClient, RtgsPublisherOptions options)
	: base(logger, paymentClient, options)
	{
	}

	public Task<SendResult> SendIdCryptInvitationAsync(IdCryptInvitationV1 message, CancellationToken cancellationToken) =>
		SendMessage(message, "idcrypt.invitation.v1", cancellationToken);
}
