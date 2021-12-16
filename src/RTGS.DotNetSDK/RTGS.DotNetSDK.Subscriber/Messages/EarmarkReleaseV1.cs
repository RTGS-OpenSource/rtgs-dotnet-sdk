namespace RTGS.DotNetSDK.Subscriber.Messages;

public record EarmarkReleaseV1
{
	public Guid LockId { get; init; }
}
