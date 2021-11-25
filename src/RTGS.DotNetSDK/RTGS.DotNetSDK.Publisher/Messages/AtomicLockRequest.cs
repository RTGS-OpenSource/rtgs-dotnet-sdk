using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	/// <summary>
	/// The AtomicLockRequest class represents the initial request to trigger a transaction.
	/// </summary>
	public record AtomicLockRequest
	{
		/// <summary>
		/// Bank rtgs id, the identifier of the bank initiating the transaction.
		/// </summary>
		/// <remarks>
		/// The <c>GenericFinancialIdentification1</c> type is from nuget package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
		/// </remarks>
		public GenericFinancialIdentification1 DbtrToRtgsId { get; init; }

		/// <summary>
		/// Creditor amount - describes the value and currency of the transfer.
		/// </summary>
		/// <remarks>
		/// The <c>ActiveCurrencyAndAmount</c> type is from nuget package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
		/// </remarks>
		public ActiveCurrencyAndAmount CdtrAmt { get; init; }

		/// <summary>
		/// Ultimate debtor account
		/// </summary>
		/// <remarks>
		/// The <c>CashAccount38</c> type is from nuget package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
		/// </remarks>
		public CashAccount38 UltmtDbtrAcct { get; init; }

		/// <summary>
		/// Ultimate creditor account
		/// </summary>
		/// <remarks>
		/// The <c>CashAccount38</c> type is from nuget package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
		/// </remarks>
		public CashAccount38 UltmtCdtrAcct { get; init; }

		/// <summary>
		/// Supplementary data
		/// <br/>This field is optional 
		/// </summary>
		public string SplmtryData { get; init; }

		/// <summary>
		/// End to end id, typically a GUID used to correlate an AtomicLockRequest with its LockResponse
		/// </summary>
		public string EndToEndId { get; init; }
	}
}
