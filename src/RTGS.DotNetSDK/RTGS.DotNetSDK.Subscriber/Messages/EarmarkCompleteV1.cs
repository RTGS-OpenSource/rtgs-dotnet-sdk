namespace RTGS.DotNetSDK.Subscriber.Messages;

public record EarmarkCompleteV1
{
	public Guid LockId { get; init; }
}
