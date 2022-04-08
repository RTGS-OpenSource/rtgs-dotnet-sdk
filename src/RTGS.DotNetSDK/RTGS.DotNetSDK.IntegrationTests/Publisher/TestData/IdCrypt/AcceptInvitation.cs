using System.Text.Json;
using RTGS.IDCryptSDK.Connections.Models;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal static class AcceptInvitation
{
	public const string Path = "/connections/6dd0dd5b-39e2-402d-aca0-890780241ede/accept-invitation";

	private static ConnectionResponse Response => new()
	{
		Alias = "385ba215-7d4e-4cdc-a7a7-f14955741e70",
		ConnectionId = "6dd0dd5b-39e2-402d-aca0-890780241ede",
		State = "request"
	};

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, JsonSerializer.Serialize(Response));
}
