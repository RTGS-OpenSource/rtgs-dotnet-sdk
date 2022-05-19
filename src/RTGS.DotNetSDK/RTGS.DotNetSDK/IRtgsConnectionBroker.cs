namespace RTGS.DotNetSDK;

/// <summary>
/// The IRtgsConnectionBroker interface, implementations of this interface are responsible for requesting a new invitation from ID Crypt and sending it to RTGS.
/// </summary>
public interface IRtgsConnectionBroker
{
	/// <summary>
	/// Calls the ID Crypt Cloud Agent to create a new invitation, and sends it to RTGS.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The result of the operation</returns>
	Task<SendResult> SendInvitationAsync(CancellationToken cancellationToken = default);
}
