using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	/// <summary>
	/// The AtomicLockRequest class
	/// </summary>
	public record AtomicLockRequest
	{
		/// <summary>
		/// Bank rtgs id
		/// </summary>
		public GenericFinancialIdentification1 DbtrToRtgsId { get; init; }

		/// <summary>
		/// Creditor amount
		/// </summary>
		public ActiveCurrencyAndAmount CdtrAmt { get; init; }

		/// <summary>
		/// Ultimate debtor account
		/// </summary>
		public CashAccount38 UltmtDbtrAcct { get; init; }

		/// <summary>
		/// Ultimate creditor account
		/// </summary>
		public CashAccount38 UltmtCdtrAcct { get; init; }

		/// <summary>
		/// Supplementary data
		/// </summary>
		public string SplmtryData { get; init; }

		/// <summary>
		/// End to end id
		/// </summary>
		public string EndToEndId { get; init; }
	}
}
