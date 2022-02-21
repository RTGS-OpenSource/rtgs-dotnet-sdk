using IDCryptGlobal.Cloud.Agent.Identity.Connection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;

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
		JsonSerializer.Serialize(ConnectionInviteResponse);

	public static string PublicDIDResponseJson => JsonSerializer.Serialize(new DIDCreated { Result = new DIDInformation { DID = "Test Did" } });

	public class DIDCreated
	{
		[JsonPropertyName("result")]
		public DIDInformation Result { get; set; }

	}

	public class DIDInformation
	{
		[JsonPropertyName("did")]
		public string DID { get; set; }

		[JsonPropertyName("verkey")]
		public string Verkey { get; set; }

		[JsonPropertyName("key_type")]
		public string KeyType { get; set; }

		[JsonPropertyName("method")]
		public string Method { get; set; }

		[JsonPropertyName("posture")]
		public string Posture { get; set; }
	}
}
