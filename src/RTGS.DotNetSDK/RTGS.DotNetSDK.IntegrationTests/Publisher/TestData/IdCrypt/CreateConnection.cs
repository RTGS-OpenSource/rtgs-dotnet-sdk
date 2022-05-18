using System.Text.Json;
using RTGS.IDCrypt.Service.Contracts.Connection;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal static class CreateConnection
{
	public const string Path = "/api/Connection";

	public static CreateConnectionInvitationResponse Response => new()
	{
		ConnectionId = "connection-id",
		Alias = "alias",
		AgentPublicDid = "agent-public-did",
		InvitationUrl = "invitation-url",
		Invitation = new ConnectionInvitation
		{
			Id = "id",
			Type = "type",
			Did = "did",
			Label = "label",
			ImageUrl = "image-url",
			RecipientKeys = new[]
			{
				"recipient-key-1",
				"recipient-key-2",
			},
			ServiceEndpoint = "service-endpoint"
		}
	};

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, JsonSerializer.Serialize(Response));
}
