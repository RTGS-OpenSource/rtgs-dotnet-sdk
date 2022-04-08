namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal static class GetPublicDid
{
	public const string Path = "/wallet/did/public";

	public const string ExpectedDid = "Test Did";

	private static string SerialisedResponse => $@"{{ 
		""result"": {{ 
			""did"": ""{ExpectedDid}"", 
			""verkey"": ""verkey"", 
			""key_type"": ""key_type"", 
			""method"": ""method"", 
			""posture"": ""posture"" 
		}} 
	}}";

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, SerialisedResponse);
}
