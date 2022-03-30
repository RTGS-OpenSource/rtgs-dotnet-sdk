using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.Messages;

/// <summary>
/// Represents an atomic lock response.
/// </summary>
public record AtomicLockResponseV1
{
	/// <summary>
	/// LockId: The id of the lock.
	/// </summary>
	public Guid LckId { get; init; }

	/// <summary>
	/// LockExpiry: When the lock expires.
	/// </summary>
	public DateTimeOffset LckXpry { get; init; }

	/// <summary>
	/// StatusCode: The response status code.
	/// </summary>
	public ResponseStatusCodes StsCd { get; init; }

	/// <summary>
	/// Message: The message.
	/// </summary>
	public string Msg { get; init; }

	/// <summary>
	/// DebtorAmount: the converted amount in the debtor currency.
	/// </summary>
	/// <remarks>
	/// The <c>ActiveCurrencyAndAmount</c> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	public ActiveCurrencyAndAmount DbtrAmt { get; init; }

	/// <summary>
	/// ExchangeRate: the value used to calculate the debtor amount.
	/// </summary>
	public decimal XchgRate { get; init; }

	/// <summary>
	/// ChargesInformation: details about any charges that will be applied.
	/// </summary>
	/// <remarks>
	/// The <c>Charges7</c> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	public Charges7 ChrgsInf { get; init; }

	/// <summary>
	/// The end to end id.
	/// </summary>
	public string EndToEndId { get; init; }
}
