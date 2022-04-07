using System.Net;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

internal record MockHttpResponse
{
	public string Path { get; init; }
	public string Content { get; init; }
	public HttpStatusCode HttpStatusCode { get; init; }
}
