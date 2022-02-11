using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.Messages;

/// <summary>
/// Represents an atomic transfer response.
/// </summary>
public class AtomicTransferResponseV1
{
	/// <summary>
	/// The PACS008 message used for the transfer.
	/// </summary>
	/// <remarks>
	/// The <see cref="FIToFICustomerCreditTransferV10" /> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	public FIToFICustomerCreditTransferV10 FullFIToFICstmrCdtTrf { get; init; }

	/// <summary>
	/// The response status code.
	/// </summary>
	public ResponseStatusCodes StatusCode { get; init; }

	/// <summary>
	/// The message.
	/// </summary>
	public string Message { get; init; }

	/// <summary>
	/// The supplementary data included in the transfer.
	/// </summary>
	/// <remarks>
	/// The <see cref="SupplementaryData1" /> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	public SupplementaryData1[] SplmtryData { get; init; }

	/// <summary>
	/// The id of the lock.
	/// </summary>
	/// <remarks>
	/// Guid value represented as a string.
	/// </remarks>
	public string LckId { get; init; }
}
