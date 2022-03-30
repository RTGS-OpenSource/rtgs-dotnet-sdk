namespace RTGS.DotNetSDK.Subscriber.Messages;

/// <summary>
/// Represents an atomic transfer response.
/// </summary>
public class AtomicTransferResponseV1
{
	/// <summary>
	/// StatusCode: The response status code.
	/// </summary>
	public ResponseStatusCodes StsCd { get; init; }

	/// <summary>
	/// Message: The message.
	/// </summary>
	public string Msg { get; init; }

	/// <summary>
	/// LockId: The id of the lock.
	/// </summary>
	public Guid LckId { get; init; }
}
