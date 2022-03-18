namespace RTGS.DotNetSDK.IntegrationTests.InternalMessages;

public record IdCryptInvitationV1
{
	public string Alias { get; init; }
	public string Label { get; init; }
	public IEnumerable<string> RecipientKeys { get; init; }
	public string Id { get; init; }
	public string Type { get; init; }
	public string ServiceEndPoint { get; init; }
	public string AgentPublicDid { get; set; }
}
