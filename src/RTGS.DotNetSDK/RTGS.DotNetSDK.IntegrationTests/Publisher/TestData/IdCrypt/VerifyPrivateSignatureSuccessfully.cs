namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal static class VerifyPrivateSignatureSuccessfully
{
	public const string Path = "/json-signatures/verify/connection-did";

	public const bool ExpectedResponse = true;

	private static string SerialisedResponse =>
		$@"{{
			""verified"": {ExpectedResponse.ToString().ToLower()}
		}}";

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, SerialisedResponse);
}
