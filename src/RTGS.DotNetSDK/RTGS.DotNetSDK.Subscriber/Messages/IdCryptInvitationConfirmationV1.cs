namespace RTGS.DotNetSDK.Subscriber.Messages;

/// <summary>
/// Represents a confirmation for an accepted inviation.
/// </summary>
public record IdCryptInvitationConfirmationV1
{
	/// <summary>
	/// The ConnectionId of the accepted and confirmed invitation.
	/// </summary>
	public string ConnectionId { get; init; }
}

