namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal static class VerifyPublicSignatureSuccessfully
{
	public const string Path = "/json-signatures/verify/public-did";

	public const bool ExpectedResponse = true;

	private static string SerialisedResponse =>
		$@"{{
			""verified"": {ExpectedResponse.ToString().ToLower()}
		}}";

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, SerialisedResponse);
}
