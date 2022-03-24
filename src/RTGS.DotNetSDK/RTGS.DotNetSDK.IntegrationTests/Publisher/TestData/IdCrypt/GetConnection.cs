using IDCryptGlobal.Cloud.Agent.Identity.Connection;
using Newtonsoft.Json;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal static class GetConnection
{
	public const string Path = "/connections/6dd0dd5b-39e2-402d-aca0-890780241ede";

	private static ConnectionAccepted Response(string state) => new()
	{
		Alias = "385ba215-7d4e-4cdc-a7a7-f14955741e70",
		ConnectionID = "6dd0dd5b-39e2-402d-aca0-890780241ede",
		State = state
	};

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, JsonConvert.SerializeObject(Response("active")));

	public static HttpRequestResponseContext HttpRequestResponseContextWithState(string state) =>
		new(Path, JsonConvert.SerializeObject(Response(state)));
}
