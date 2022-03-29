namespace RTGS.DotNetSDK.Publisher.Messages;

/// <summary>
/// Used to represent the message sent to RTGS to indicate that the funds have been transferred (or not if <see cref="Sccs"/> = false)
/// </summary>
public class AtomicTransferConfirmationV1
{
	/// <summary>
	/// The lock Id used to correlate this transfer with the original atomic lock request.
	/// </summary>
	public Guid LckId { get; init; }

	/// <summary>
	/// True to confirm, false otherwise.
	/// </summary>
	public bool Sccs { get; init; }
}
