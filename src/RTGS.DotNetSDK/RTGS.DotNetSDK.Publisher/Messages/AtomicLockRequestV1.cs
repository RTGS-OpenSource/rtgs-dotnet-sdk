using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.Messages;

/// <summary>
/// Represents an atomic lock request.
/// </summary>
public record AtomicLockRequestV1
{
	/// <summary>
	/// Bank RTGS id, the identifier of the bank initiating the transaction.
	/// </summary>
	/// <remarks>
	/// The <c>GenericFinancialIdentification1</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public GenericFinancialIdentification1 DbtrToRtgsId { get; init; }

	/// <summary>
	/// Creditor amount - describes the value and currency of the transfer.
	/// </summary>
	/// <remarks>
	/// The <c>ActiveCurrencyAndAmount</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public ActiveCurrencyAndAmount CdtrAmt { get; init; }

	/// <summary>
	/// Ultimate debtor account.
	/// </summary>
	/// <remarks>
	/// The <c>CashAccount40</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public CashAccount40 UltmtDbtrAcct { get; init; }

	/// <summary>
	/// Ultimate creditor account.
	/// </summary>
	/// <remarks>
	/// The <c>CashAccount40</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public CashAccount40 UltmtCdtrAcct { get; init; }

	/// <summary>
	/// Supplementary data.
	/// <br/>This field is optional.
	/// </summary>
	public string SplmtryData { get; init; }

	/// <summary>
	/// End to end id, typically a GUID used to correlate an atomic lock request with its response.
	/// </summary>
	public string EndToEndId { get; init; }
}
