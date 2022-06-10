using System.Text.Json;
using RTGS.IDCrypt.Service.Contracts.Message.Verify;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal static class VerifyOwnMessageSuccessfully
{
	public const string Path = "/api/message/verify/own";

	public static VerifyOwnMessageResponse Response => new()
	{
		Verified = true
	};

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, JsonSerializer.Serialize(Response));
}
