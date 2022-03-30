namespace RTGS.DotNetSDK.Publisher.Messages;

/// <summary>
/// Used to represent the message sent to RTGS to indicate that the funds have been earmarked (or not if <see cref="Sccs"/> = <code>false</code>)
/// </summary>
public record EarmarkConfirmationV1
{
	/// <summary>
	/// LockId: The lock Id for which to initiate earmark confirmation.
	/// </summary>
	public Guid LckId { get; init; }

	/// <summary>
	/// Success: True to confirm, false otherwise.
	/// </summary>
	public bool Sccs { get; init; }
}
