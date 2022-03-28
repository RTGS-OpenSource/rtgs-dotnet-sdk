using IDCryptGlobal.Cloud.Agent.Identity.Connection;
using Newtonsoft.Json;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal static class CreateInvitation
{
	public const string Path = "/connections/create-invitation";

	public static ConnectionInviteResponseModel Response => new()
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

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path,
			JsonConvert.SerializeObject(Response));
}
