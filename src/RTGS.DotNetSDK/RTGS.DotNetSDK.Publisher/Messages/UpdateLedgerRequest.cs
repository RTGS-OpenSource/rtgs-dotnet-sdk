using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	/// <summary>
	/// Represents the message sent to RTGS to indicate a change to the available funds of a bank.
	/// TODO : Is the amount a delta (+/-) or total 
	/// </summary>
	public class UpdateLedgerRequest
	{
		/// <summary>
		/// International bank account number
		/// </summary>
		public string IBAN { get; init; }

		/// <summary>
		/// Bank Rtgs Id - identifier of the bank whose available funds have changed.
		/// </summary>
		public GenericFinancialIdentification1 BkToRtgsId { get; init; }

		/// <summary>
		/// The amount now available, represented by units and nano units.
		/// </summary>
		public ProtoDecimal Amt { get; init; }
	}
}
