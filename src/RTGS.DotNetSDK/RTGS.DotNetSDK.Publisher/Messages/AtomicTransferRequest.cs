using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	/// <summary>
	/// The AtomicTransferRequest class
	/// </summary>
	public record AtomicTransferRequest
	{
		/// <summary>
		/// Bank rtgs id
		/// </summary>
		public GenericFinancialIdentification1 DbtrToRtgsId { get; init; }

		/// <summary>
		/// Financial institution to financial institution customer credit transfer
		/// </summary>
		public FinancialInstitutionToFinancialInstitutionCustomerCreditTransfer FIToFICstmrCdtTrf { get; init; }

		/// <summary>
		/// Lock Id
		/// </summary>
		public string LckId { get; init; }
	}
}
