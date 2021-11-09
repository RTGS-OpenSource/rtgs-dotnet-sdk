using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	public record AtomicLockRequest
	{
		// TODO: properties
		public GenericFinancialIdentification1 DbtrToRtgsId { get; init; }
		public ActiveCurrencyAndAmount CdtrAmt { get; init; }
		public CashAccount38 UltmtDbtrAcct { get; init; }
		public CashAccount38 UltmtCdtrAcct { get; init; }
		public string SplmtryData { get; init; }
		public string EndToEndId { get; init; }
	}
}
