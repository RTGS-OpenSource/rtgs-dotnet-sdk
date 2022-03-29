using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.Messages;

/// <summary>
/// Represents an atomic transfer funds message.
/// </summary>
public record AtomicTransferFundsV1
{
	/// <summary>
	/// The PACS008 message.
	/// </summary>
	public FIToFICustomerCreditTransferV10 FIToFICstmrCdtTrf { get; set; }

	/// <summary>
	/// The id of the lock.
	/// </summary>
	public Guid LckId { get; set; }
}
