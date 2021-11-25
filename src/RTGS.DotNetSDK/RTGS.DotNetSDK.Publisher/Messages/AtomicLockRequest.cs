using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	/// <summary>
	/// The AtomicLockRequest class represents the initial request to earmark funds
	/// </summary>
	public record AtomicLockRequest
	{
		/// <summary>
		/// Bank rtgs id, the identifier of the bank initiating the transaction.
		/// </summary>
		public GenericFinancialIdentification1 DbtrToRtgsId { get; init; }

		/// <summary>
		/// Creditor amount - describes the value and currency of the transfer.
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
		/// End to end id, typically a GUID used to correlate an AtomicLockRequest with its LockResponse
		/// </summary>
		public string EndToEndId { get; init; }
	}
}
