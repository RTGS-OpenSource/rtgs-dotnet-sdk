namespace RTGS.DotNetSDK.Subscriber.Messages;

/// <summary>
/// Represents an atomic transfer funds message.
/// </summary>
public record AtomicTransferFundsV1
{
	/// <summary>
	/// The PACS008 message represented using JSON.
	/// </summary>
	public string PacsJson { get; set; }

	/// <summary>
	/// The id of the lock.
	/// </summary>
	public Guid LockId { get; set; }
}
