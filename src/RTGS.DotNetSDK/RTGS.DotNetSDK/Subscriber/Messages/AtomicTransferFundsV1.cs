using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.Messages;

/// <summary>
/// Represents an atomic transfer funds message.
/// </summary>
public record AtomicTransferFundsV1
{
	/// <summary>
	/// FIToFICustomerCreditTransfer: Financial institution to financial institution customer credit transfer.
	/// </summary>
	/// <remarks>
	/// The <c>FIToFICustomerCreditTransferV10</c> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	public FIToFICustomerCreditTransferV10 FIToFICstmrCdtTrf { get; set; }

	/// <summary>
	/// LockId: The id of the lock.
	/// </summary>
	public Guid LckId { get; set; }
}
