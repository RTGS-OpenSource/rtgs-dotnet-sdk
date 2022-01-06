using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.Messages;

/// <summary>
/// Represents an atomic lock response.
/// </summary>
public record AtomicLockResponseV1
{
	/// <summary>
	/// The id of the lock.
	/// </summary>
	public string LckId { get; init; }

	/// <summary>
	/// When the lock expires.
	/// </summary>
	public string LckExpry { get; init; }

	/// <summary>
	/// The response status code.
	/// </summary>
	public ResponseStatusCodes StatusCode { get; init; }

	/// <summary>
	/// The message.
	/// </summary>
	public string Message { get; init; }

	/// <summary>
	/// Debtor amount - the converted amount in the debtor currency.
	/// </summary>
	/// <remarks>
	/// The <see cref="ActiveCurrencyAndAmount"/> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	public ActiveCurrencyAndAmount DbtrAmt { get; init; }

	/// <summary>
	/// Exchange rate - the value used to calculate the debtor amount.
	/// </summary>
	public decimal XchgRate { get; init; }

	/// <summary>
	/// Changes information - details about any changes that will be applied.
	/// </summary>
	/// <remarks>
	/// The <see cref="Charges7"/> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	public Charges7 ChrgsInf { get; init; }

	/// <summary>
	/// The end to end id.
	/// </summary>
	public string EndToEndId { get; init; }
}
