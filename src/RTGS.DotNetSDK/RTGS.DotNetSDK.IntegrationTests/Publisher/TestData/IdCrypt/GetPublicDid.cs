using IDCryptGlobal.Cloud.Agent.Identity.Vault;
using Newtonsoft.Json;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal static class GetPublicDid
{
	public const string Path = "/wallet/did/public";

	public static DIDCreated Response => new()
	{
		Result = new DIDInformation
		{
			DID = "Test Did"
		}
	};

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, JsonConvert.SerializeObject(Response));
}
