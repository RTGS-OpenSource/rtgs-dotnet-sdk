using RTGS.IDCryptSDK.Connections.Models;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

public class GetActiveConnectionWithAlias
{
	public const string Path = "/connections";

	public static ConnectionResponse ExpectedResponse => new()
	{
		Accept = "accept",
		Alias = "alias",
		ConnectionId = "connection-id",
		ConnectionProtocol = "connection-protocol",
		CreatedAt = "created-at",
		InvitationKey = "invitation-key",
		InvitationMessageId = "invitation-message-id",
		InvitationMode = "invitation-mode",
		MyDid = "my-did",
		RequestId = "request-id",
		Rfc23State = "rfc-23-state",
		RoutingState = "routing-state",
		State = "state",
		TheirDid = "their-did",
		TheirLabel = "their-label",
		TheirRole = "their-role",
		UpdatedAt = "updated-at"
	};

	private static string SerialisedResponse =>
		$@"{{ 
			""results"": [
				{{
					""accept"": ""{ExpectedResponse.Accept}"",
					""alias"": ""{ExpectedResponse.Alias}"",
					""connection_id"": ""{ExpectedResponse.ConnectionId}"",
					""connection_protocol"": ""{ExpectedResponse.ConnectionProtocol}"",
					""created_at"": ""{ExpectedResponse.CreatedAt}"",
					""invitation_key"": ""{ExpectedResponse.InvitationKey}"",
					""invitation_msg_id"": ""{ExpectedResponse.InvitationMessageId}"",
					""invitation_mode"": ""{ExpectedResponse.InvitationMode}"",
					""my_did"": ""{ExpectedResponse.MyDid}"",
					""request_id"": ""{ExpectedResponse.RequestId}"",
					""rfc23_state"": ""{ExpectedResponse.Rfc23State}"",
					""routing_state"": ""{ExpectedResponse.RoutingState}"",
					""state"": ""{ExpectedResponse.State}"",
					""their_did"": ""{ExpectedResponse.TheirDid}"",
					""their_label"": ""{ExpectedResponse.TheirLabel}"",
					""their_role"": ""{ExpectedResponse.TheirRole}"",
					""updated_at"": ""{ExpectedResponse.UpdatedAt}""
				}}
			]
		}}";

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, SerialisedResponse);
}
