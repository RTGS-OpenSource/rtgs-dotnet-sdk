using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.Messages;

/// <summary>
/// Represents an earmark funds message.
/// </summary>
public record EarmarkFundsV1
{
	/// <summary>
	/// LockId: The id of the lock.
	/// </summary>
	public Guid LckId { get; init; }

	/// <summary>
	/// Account: The account to use when earmarking funds.
	/// </summary>
	/// <remarks>
	/// The <c>CashAccount40</c> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	public CashAccount40 Acct { get; init; }

	/// <summary>
	/// Amount: The amount to earmark.
	/// </summary>
	/// <remarks>
	/// The <c>ActiveCurrencyAndAmount</c> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	public ActiveCurrencyAndAmount Amt { get; init; }
}
