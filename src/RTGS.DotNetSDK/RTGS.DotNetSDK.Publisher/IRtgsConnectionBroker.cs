
namespace RTGS.DotNetSDK.Publisher;

public interface IRtgsConnectionBroker
{
	Task<SendInvitationResult> SendInvitationAsync(CancellationToken cancellationToken = default);
}
