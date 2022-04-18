namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal static class VerifyPublicSignatureUnsuccessfully
{
	public const string Path = "/json-signatures/verify/public-did";

	public const bool ExpectedResponse = false;

	private static string SerialisedResponse =>
		$@"{{
			""verified"": {ExpectedResponse.ToString().ToLower()}
		}}";

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, SerialisedResponse);
}
