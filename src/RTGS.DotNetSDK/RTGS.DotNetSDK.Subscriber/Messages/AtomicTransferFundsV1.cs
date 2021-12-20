namespace RTGS.DotNetSDK.Subscriber.Messages;

public record AtomicTransferFundsV1
{
	public string PacsJson { get; set; }

	public Guid LockId { get; set; }
}
