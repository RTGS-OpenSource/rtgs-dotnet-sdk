using System.Text.Json;
using RTGS.IDCrypt.Service.Contracts.Message.Verify;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal static class VerifyMessageUnsuccessfully
{
	public const string Path = "/api/message/verify";

	public static VerifyResponse Response => new()
	{
		Verified = false
	};

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, JsonSerializer.Serialize(Response));
}
