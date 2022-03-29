using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.Messages;

/// <summary>
/// Represents the message sent to RTGS to indicate a change to the available funds of a bank.
/// </summary>
public class UpdateLedgerRequestV1
{
	/// <summary>
	/// IBAN (International Bank Account Number).
	/// </summary>
	public AccountIdentification4Choice AcctId { get; init; }

	/// <summary>
	/// The amount now available.
	/// </summary>
	public ActiveCurrencyAndAmount Amt { get; init; }
}
