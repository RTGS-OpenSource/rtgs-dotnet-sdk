namespace RTGS.DotNetSDK.Publisher;

/// <summary>
/// Represents the result of creating an ID Crypt invitation and sending it to RTGS.
/// </summary>
public record SendInvitationResult
{
	/// <summary>
	/// The unique alias for this invitation.
	/// Note, this will be null if sending the invitation to RTGS does not result in 'Success'.
	/// </summary>
	public string Alias { get; internal init; }

	/// <summary>
	/// The connection ID for this invitation.
	/// Note, this will be null if sending the invitation to RTGS does not result in 'Success'.
	/// </summary>
	public string ConnectionId { get; internal init; }

	/// <summary>
	/// The result of sending the invitation to RTGS.
	/// </summary>
	public SendResult SendResult { get; internal init; }
}
