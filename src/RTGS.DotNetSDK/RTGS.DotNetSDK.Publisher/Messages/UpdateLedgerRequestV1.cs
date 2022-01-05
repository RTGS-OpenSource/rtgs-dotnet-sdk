namespace RTGS.DotNetSDK.Publisher.Messages;

/// <summary>
/// Represents the message sent to RTGS to indicate a change to the available funds of a bank.
/// </summary>
public class UpdateLedgerRequestV1
{
	/// <summary>
	/// IBAN (International Bank Account Number).
	/// </summary>
	public string AccountIdentifier { get; init; }


	/// <summary>
	/// The amount now available.
	/// </summary>
	public decimal Amount { get; init; }
}
