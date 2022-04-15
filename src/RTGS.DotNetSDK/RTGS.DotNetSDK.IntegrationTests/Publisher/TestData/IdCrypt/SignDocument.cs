using System.Text.Json;
using RTGS.IDCryptSDK.JsonSignatures.Models;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal class SignDocument
{
	public const string Path = "/json-signatures/sign";

	public static SignDocumentResponse Response => new()
	{
		PairwiseDidSignature = "pairwise-did-signature",
		PublicDidSignature = "public-did-signature"
	};

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, JsonSerializer.Serialize(Response));
}
