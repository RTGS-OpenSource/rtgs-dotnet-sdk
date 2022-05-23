using System.Text.Json;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal class SignMessage
{
	public const string Path = "/api/message/sign";

	public static SignMessageResponse Response => new()
	{
		PublicDidSignature = "public-did-signature",
		PairwiseDidSignature = "pairwise-did-signature",
		Alias = "id-crypt-alias"
	};

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, JsonSerializer.Serialize(Response));
}
