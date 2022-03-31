using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.Messages;

/// <summary>
/// Represents the message sent to RTGS to indicate a change to the available funds of a bank.
/// </summary>
public class UpdateLedgerRequestV1
{
	/// <summary>
	/// AccountIdentifier: Identifier for the account related to this change.
	/// </summary>
	/// <remarks>
	/// The <c>AccountIdentification4Choice</c> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	public AccountIdentification4Choice AcctId { get; init; }

	/// <summary>
	/// Amount: The amount now available.
	/// </summary>
	/// <remarks>
	/// The <c>ActiveCurrencyAndAmount</c> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	public ActiveCurrencyAndAmount Amt { get; init; }
}
