
namespace RTGS.DotNetSDK.Publisher;

public interface IRtgsConnectionBroker
{
	Task<string> SendInvitationAsync(CancellationToken cancellationToken = default);
}
