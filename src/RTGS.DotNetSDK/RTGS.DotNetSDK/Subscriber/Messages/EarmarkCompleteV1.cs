namespace RTGS.DotNetSDK.Subscriber.Messages;

/// <summary>
/// Represents an earmark complete message.
/// </summary>
public record EarmarkCompleteV1
{
	/// <summary>
	/// The id of the lock.
	/// </summary>
	public Guid LckId { get; init; }
}
