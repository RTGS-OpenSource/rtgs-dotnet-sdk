using System.Net.Http;
using System.Text;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;

internal class IdCryptEndPoints
{
	public const string InvitationPath = "/connections/create-invitation";
	public const string PublicDidPath = "/wallet/did/public";

	public static readonly Dictionary<string, HttpContent> HttpContentsMock = new()
	{
		{
			InvitationPath,
			new StringContent(IdCryptTestMessages.ConnectionInviteResponseJson, Encoding.UTF8)
		},
		{
			PublicDidPath,
			new StringContent(IdCryptTestMessages.AgentPublicDidResponseJson, Encoding.UTF8)
		}
	};
}
