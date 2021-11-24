using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	/// <summary>
	/// The UpdateLedgerRequest class
	/// </summary>
	public class UpdateLedgerRequest
	{
		/// <summary>
		/// International bank account number
		/// </summary>
		public string IBAN { get; init; }

		/// <summary>
		/// Bank Rtgs Id
		/// </summary>
		public GenericFinancialIdentification1 BkToRtgsId { get; init; }

		/// <summary>
		/// Amount
		/// </summary>
		public ProtoDecimal Amt { get; init; }
	}
}
