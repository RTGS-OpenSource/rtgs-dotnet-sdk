namespace RTGS.DotNetSDK.Subscriber.Messages;

/// <summary>
/// Represents an atomic transfer response.
/// </summary>
public class AtomicTransferResponseV1
{
	/// <summary>
	/// The response status code.
	/// </summary>
	public ResponseStatusCodes StatusCode { get; init; }

	/// <summary>
	/// The message.
	/// </summary>
	public string Message { get; init; }

	/// <summary>
	/// The id of the lock.
	/// </summary>
	public Guid LockId { get; init; }
}
