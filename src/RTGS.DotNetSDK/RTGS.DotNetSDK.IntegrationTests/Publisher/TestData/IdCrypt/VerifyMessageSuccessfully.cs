using System.Text.Json;
using RTGS.IDCrypt.Service.Contracts.Message.Verify;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal static class VerifyMessageSuccessfully
{
	public const string Path = "/api/message/verify";

	public static VerifyResponse Response => new()
	{
		Verified = true
	};

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, JsonSerializer.Serialize(Response));
}
