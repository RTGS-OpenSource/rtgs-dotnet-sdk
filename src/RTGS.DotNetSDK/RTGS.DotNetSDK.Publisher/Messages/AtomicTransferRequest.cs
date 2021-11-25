using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	/// <summary>
	/// Represents the message used to trigger the funds to be transferred
	/// </summary>
	public record AtomicTransferRequest
	{
		/// <summary>
		/// Bank rtgs id, unique identifier of the debtor (partner) bank.
		/// </summary>
		public GenericFinancialIdentification1 DbtrToRtgsId { get; init; }

		/// <summary>
		/// Financial institution to financial institution customer credit transfer. TODO: can we describe this in better english??
		/// </summary>
		public FinancialInstitutionToFinancialInstitutionCustomerCreditTransfer FIToFICstmrCdtTrf { get; init; }

		/// <summary>
		/// Lock Id - a GUID to link this Block (Transfer) to its corresponding Lock that was taken earlier in the transaction.
		/// </summary>
		public string LckId { get; init; }
	}
}
