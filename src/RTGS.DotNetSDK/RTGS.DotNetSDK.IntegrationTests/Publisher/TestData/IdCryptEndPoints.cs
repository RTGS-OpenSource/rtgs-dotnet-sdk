using System.Net;
using System.Net.Http;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

internal class IdCryptEndPoints
{
	public const string InvitationPath = "/connections/create-invitation";
	public const string PublicDidPath = "/wallet/did/public";
	public const string ReceiveInvitationPath = "/connections/receive-invitation";
	public const string AcceptInvitationPath = "/connections/6dd0dd5b-39e2-402d-aca0-890780241ede/accept-invitation";
	public const string GetConnectionPath = "/connections/6dd0dd5b-39e2-402d-aca0-890780241ede";

	public static readonly List<MockHttpResponse> MockHttpResponses = new()
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
		},
		new MockHttpResponse
		{
			Content = new StringContent(IdCryptTestMessages.ReceiveInvitationResponseJson),
			HttpStatusCode = HttpStatusCode.OK,
			Path = ReceiveInvitationPath
		},
		new MockHttpResponse
		{
			Content = new StringContent(IdCryptTestMessages.ConnectionAcceptedResponseJson),
			HttpStatusCode = HttpStatusCode.OK,
			Path = AcceptInvitationPath
		},
		new MockHttpResponse
		{
			Content = new StringContent(IdCryptTestMessages.GetConnectionResponseJson),
			HttpStatusCode = HttpStatusCode.OK,
			Path = GetConnectionPath
		}
	};
}
