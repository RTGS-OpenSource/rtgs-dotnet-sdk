namespace RTGS.DotNetSDK.Subscriber.Messages;

/// <summary>
/// Represents an atomic transfer response.
/// </summary>
public class AtomicTransferResponseV1
{
	/// <summary>
	/// The response status code.
	/// </summary>
	public ResponseStatusCodes StsCd { get; init; }

	/// <summary>
	/// The message.
	/// </summary>
	public string Msg { get; init; }

	/// <summary>
	/// The id of the lock.
	/// </summary>
	public Guid LckId { get; init; }
}
