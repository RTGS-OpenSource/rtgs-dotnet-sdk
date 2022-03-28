using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.Messages;

/// <summary>
/// Represents the message used to trigger the funds to be transferred.
/// </summary>
public record AtomicTransferRequestV1
{
	/// <summary>
	/// Financial institution to financial institution customer credit transfer.
	/// </summary>
	/// <remarks>
	/// The <c>FIToFICustomerCreditTransferV10</c> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	public FIToFICustomerCreditTransferV10 FIToFICstmrCdtTrf { get; init; }

	/// <summary>
	/// Lock Id - a GUID to link this atomic transfer to its corresponding atomic lock.
	/// </summary>
	public Guid LockId { get; init; }
}
