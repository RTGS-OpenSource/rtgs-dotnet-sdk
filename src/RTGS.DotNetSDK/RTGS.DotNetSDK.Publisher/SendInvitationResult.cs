namespace RTGS.DotNetSDK.Publisher;

public record SendInvitationResult
{
	public string ConnectionId { get; internal set; }
	public SendResult SendResult { get; internal set; }
}
