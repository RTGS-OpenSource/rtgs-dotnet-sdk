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
	/// <remarks>
	/// Guid value represented as a string.
	/// </remarks>
	public string LckId { get; init; }
}
