namespace RTGS.DotNetSDK.Subscriber.Messages;

/// <summary>
/// Represents an earmark release message.
/// </summary>
public record EarmarkReleaseV1
{
	/// <summary>
	/// The id of the lock.
	/// </summary>
	public Guid LockId { get; init; }
}
