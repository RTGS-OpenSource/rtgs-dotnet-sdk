using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Publisher.Messages;

/// <summary>
/// Represents the message used to trigger the funds to be transferred.
/// </summary>
public record AtomicTransferRequestV1
{
	/// <summary>
	/// Bank RTGS id, identifier of the bank.
	/// </summary>
	/// <remarks>
	/// The <c>GenericFinancialIdentification1</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public GenericFinancialIdentification1 DbtrToRtgsId { get; init; }

	/// <summary>
	/// Financial institution to financial institution customer credit transfer.
	/// </summary>
	/// <remarks>
	/// The <c>FinancialInstitutionToFinancialInstitutionCustomerCreditTransfer</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public FinancialInstitutionToFinancialInstitutionCustomerCreditTransfer FIToFICstmrCdtTrf { get; init; }

	/// <summary>
	/// Lock Id - a GUID to link this atomic transfer to its corresponding atomic lock.
	/// </summary>
	public string LckId { get; init; }
}
