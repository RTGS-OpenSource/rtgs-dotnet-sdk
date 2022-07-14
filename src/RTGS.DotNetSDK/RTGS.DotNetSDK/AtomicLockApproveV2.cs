using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK;

public record AtomicLockApproveV2
{
	/// <summary>
	/// LockId: The id of the lock.
	/// </summary>
	public Guid LckId { get; init; }

	/// <summary>
	/// CreditorAmount: describes the value and currency of the transfer.
	/// </summary>
	/// <remarks>
	/// The <c>ActiveCurrencyAndAmount</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public ActiveCurrencyAndAmount CdtrAmt { get; init; }

	/// <summary>
	/// DebtorAmount: describes the value and currency of the transfer.
	/// </summary>
	/// <remarks>
	/// The <c>ActiveCurrencyAndAmount</c> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public ActiveCurrencyAndAmount DbtrAmt { get; init; }

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


}
