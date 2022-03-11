using System.Net;
using System.Net.Http;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

internal record MockHttpResponse
{
	public string Path { get; init; }
	public HttpContent Content { get; init; }
	public HttpStatusCode HttpStatusCode { get; init; }
}
