using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Subscriber.Messages
{
	public class AtomicTransferResponseV1
	{
		public FinancialInstitutionToFinancialInstitutionCustomerCreditTransfer FullFIToFICstmrCdtTrf { get; init; }
		public ResponseStatusCodes StatusCode { get; init; }
		public string Message { get; init; }
		public SupplementaryData1[] SplmtryData { get; init; }
		public string LckId { get; init; }
	}
}
