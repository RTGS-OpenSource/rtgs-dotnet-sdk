using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.Messages;

/// <summary>
/// Represents an atomic lock request.
/// </summary>
public record AtomicLockRequestV1
{
	/// <summary>
	/// DebtorRtgsId: Bank RTGS id, the identifier of the bank initiating the transaction.
	/// </summary>
	/// <remarks>
	/// The <c>GenericFinancialIdentification1</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public GenericFinancialIdentification1 DbtrRtgsId { get; init; }

	/// <summary>
	/// CreditorAmount: describes the value and currency of the transfer.
	/// </summary>
	/// <remarks>
	/// The <c>ActiveCurrencyAndAmount</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public ActiveCurrencyAndAmount CdtrAmt { get; init; }

	/// <summary>
	/// DebtorAccount: Debtor account details.
	/// </summary>
	/// <remarks>
	/// The <c>CashAccount40</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public CashAccount40 DbtrAcct { get; init; }

	/// <summary>
	/// DebtorAgentAccount: Debtor agent account details.
	/// </summary>
	/// <remarks>
	/// The <c>CashAccount40</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public CashAccount40 DbtrAgntAcct { get; init; }

	/// <summary>
	/// CreditorAccount: Creditor account details.
	/// </summary>
	/// <remarks>
	/// The <c>CashAccount40</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public CashAccount40 CdtrAcct { get; init; }

	/// <summary>
	/// CreditorAgentAccount: Creditor agent account details.
	/// </summary>
	/// <remarks>
	/// The <c>CashAccount40</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public CashAccount40 CdtrAgntAcct { get; init; }

	/// <summary>
	/// UltimateDebtorAccount: Ultimate debtor account details.
	/// </summary>
	/// <remarks>
	/// The <c>CashAccount40</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public CashAccount40 UltmtDbtrAcct { get; init; }

	/// <summary>
	/// UltimateCreditorAccount: Ultimate creditor account details.
	/// </summary>
	/// <remarks>
	/// The <c>CashAccount40</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public CashAccount40 UltmtCdtrAcct { get; init; }

	/// <summary>
	/// End to end id, typically a GUID used to correlate an atomic lock request with its response.
	/// </summary>
	public string EndToEndId { get; init; }
}
