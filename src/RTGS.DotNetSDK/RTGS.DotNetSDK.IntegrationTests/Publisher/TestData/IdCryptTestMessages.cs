using IDCryptGlobal.Cloud.Agent.Identity.Connection;
using IDCryptGlobal.Cloud.Agent.Identity.Vault;
using Newtonsoft.Json;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public static class IdCryptTestMessages
{
	public static ConnectionInviteResponseModel ConnectionInviteResponse => new()
	{
		ConnectionID = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
		Invitation = new ConnectionInvitation
		{
			ID = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
			Type = "https://didcomm.org/my-family/1.0/my-message-type",
			Label = "Bob",
			RecipientKeys = new[]
			{
				"H3C2AVvLMv6gmMNam3uVAjZpfkcJCwDwnZn6z3wXmqPV"
			},
			ServiceEndPoint = "http://192.168.56.101:8020"
		}
	};

	public static string ConnectionInviteResponseJson =>
		JsonConvert.SerializeObject(ConnectionInviteResponse);

	public static DIDCreated GetPublicDidResponse => new()
	{
		Result = new DIDInformation
		{
			DID = "Test Did"
		}
	};

	public static string GetPublicDidResponseJson =>
		JsonConvert.SerializeObject(GetPublicDidResponse);

	public static ConnectionReceived ReceiveInvitationResponse => new()
	{
		Alias = "385ba215-7d4e-4cdc-a7a7-f14955741e70",
		ConnectionID = "6dd0dd5b-39e2-402d-aca0-890780241ede",
		State = "active"
	};

	public static string ReceiveInvitationResponseJson =>
		JsonConvert.SerializeObject(ReceiveInvitationResponse);

	public static ConnectionAccepted ConnectionAcceptedResponse => new()
	{
		Alias = "385ba215-7d4e-4cdc-a7a7-f14955741e70",
		ConnectionID = "6dd0dd5b-39e2-402d-aca0-890780241ede",
		State = "invitation"
	};

	public static string ConnectionAcceptedResponseJson =>
		JsonConvert.SerializeObject(ConnectionAcceptedResponse);

	public static ConnectionAccepted GetConnectionResponse => new()
	{
		Alias = "385ba215-7d4e-4cdc-a7a7-f14955741e70",
		ConnectionID = "6dd0dd5b-39e2-402d-aca0-890780241ede",
		State = "active"
	};

	public static string GetConnectionResponseJson =>
		JsonConvert.SerializeObject(GetConnectionResponse);
}
