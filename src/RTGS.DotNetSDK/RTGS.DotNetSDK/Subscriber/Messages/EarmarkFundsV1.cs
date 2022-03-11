using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.Messages;

/// <summary>
/// Represents an earmark funds message.
/// </summary>
public record EarmarkFundsV1
{
	/// <summary>
	/// The id of the lock.
	/// </summary>
	public Guid LockId { get; init; }

	/// <summary>
	/// The account to use when earmarking funds.
	/// </summary>
	/// <remarks>
	/// The <see cref="CashAccount40" /> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	public CashAccount40 LiquidityPoolAccount { get; init; }

	/// <summary>
	/// The amount to earmark.
	/// </summary>
	public decimal Amount { get; init; }
}
