namespace RTGS.DotNetSDK.IdCrypt.Messages;

/// <summary>
/// Represents a confirmation for an accepted invitation.
/// </summary>
public record IdCryptInvitationConfirmationV1
{
	/// <summary>
	/// The Alias of the accepted and confirmed invitation.
	/// </summary>
	public string Alias { get; init; }

	/// <summary>
	/// The Public DID for the ID Crypt agent that accepted the invitation.
	/// </summary>
	public string AgentPublicDid { get; init; }
}
