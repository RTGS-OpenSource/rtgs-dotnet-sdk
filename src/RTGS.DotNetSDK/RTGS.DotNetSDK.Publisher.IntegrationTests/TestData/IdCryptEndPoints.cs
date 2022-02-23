using System.Net;
using System.Net.Http;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;

internal class IdCryptEndPoints
{
	public const string InvitationPath = "/connections/create-invitation";
	public const string PublicDidPath = "/wallet/did/public";

	public static List<MockHttpResponse> MockHttpResponses = new()
	{
		new MockHttpResponse
		{
			Content = new StringContent(IdCryptTestMessages.GetPublicDidResponseJson),
			HttpStatusCode = HttpStatusCode.OK,
			Path = PublicDidPath
		},
		new MockHttpResponse
		{
			Content = new StringContent(IdCryptTestMessages.ConnectionInviteResponseJson),
			HttpStatusCode = HttpStatusCode.OK,
			Path = InvitationPath
		}
	};
}
