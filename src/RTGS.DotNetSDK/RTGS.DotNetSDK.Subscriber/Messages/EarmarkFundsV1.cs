using RTGS.Public.Payment.V1.Pacs;

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
	/// The <see cref="CashAccount38" /> type is from NuGet package RTGS.Public.Payment.Client <see href="https://www.nuget.org/packages/RTGS.Public.Payment.Client/"/>
	/// </remarks>
	public CashAccount38 LiquidityPoolAccount { get; init; }

	/// <summary>
	/// The amount to earmark.
	/// </summary>
	public decimal Amount { get; init; }
}
