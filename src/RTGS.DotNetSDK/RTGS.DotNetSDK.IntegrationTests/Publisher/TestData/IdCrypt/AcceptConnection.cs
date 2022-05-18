namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal static class AcceptConnection
{
	public const string Path = "/api/Connection/Accept";

	public static HttpRequestResponseContext HttpRequestResponseContext => new(Path, "{}");
}
