namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal static class AcceptConnection
{
	public const string Path = "/api/bank-connection/accept";

	public static HttpRequestResponseContext HttpRequestResponseContext => new(Path, "{}");
}
