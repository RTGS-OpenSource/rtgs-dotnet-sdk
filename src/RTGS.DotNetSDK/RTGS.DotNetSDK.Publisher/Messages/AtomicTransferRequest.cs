using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	/// <summary>
	/// Represents the message used to trigger the funds to be transferred
	/// </summary>
	public record AtomicTransferRequest
	{
		/// <summary>
		/// Bank rtgs id, identifier of the bank.
		/// </summary>
		/// <remarks>
		/// The <c>GenericFinancialIdentification1</c> type is from nuget package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
		/// </remarks>
		public GenericFinancialIdentification1 DbtrToRtgsId { get; init; }

		/// <summary>
		/// Financial institution to financial institution customer credit transfer. TODO: can we describe this in better english??
		/// </summary>
		/// <remarks>
		/// The <c>FinancialInstitutionToFinancialInstitutionCustomerCreditTransfer</c> type is from nuget package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
		/// </remarks>
		public FinancialInstitutionToFinancialInstitutionCustomerCreditTransfer FIToFICstmrCdtTrf { get; init; }

		/// <summary>
		/// Lock Id - a GUID to link this AtomicTransfer to its corresponding Lock.
		/// </summary>
		public string LckId { get; init; }
	}
}
