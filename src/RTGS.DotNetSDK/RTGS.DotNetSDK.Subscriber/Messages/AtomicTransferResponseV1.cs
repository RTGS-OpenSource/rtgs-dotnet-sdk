using RTGS.Public.Payment.V1.Pacs;

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
	/// The <see cref="FinancialInstitutionToFinancialInstitutionCustomerCreditTransfer" /> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public FinancialInstitutionToFinancialInstitutionCustomerCreditTransfer FullFIToFICstmrCdtTrf { get; init; }

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
	public SupplementaryData1[] SplmtryData { get; init; }

	/// <summary>
	/// The id of the lock.
	/// </summary>
	public string LckId { get; init; }
}
