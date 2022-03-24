using System.Net;
using System.Net.Http;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;

namespace RTGS.DotNetSDK.IntegrationTests.HttpHandlers;

internal class StatusCodeHttpHandlerBuilder
{
	private Dictionary<string, Queue<MockHttpResponse>> Responses { get; } = new();

	public static StatusCodeHttpHandlerBuilder Create() => new();

	public StatusCodeHttpHandlerBuilder WithServiceUnavailableResponse(string path) =>
		WithResponse(path, null, HttpStatusCode.ServiceUnavailable);

	public StatusCodeHttpHandlerBuilder WithOkResponse(HttpRequestResponseContext httpRequestResponseContext) =>
		WithResponse(
			httpRequestResponseContext.RequestPath,
			new StringContent(httpRequestResponseContext.ResponseContent),
			HttpStatusCode.OK);

	public StatusCodeHttpHandler Build() => new(Responses);

	private StatusCodeHttpHandlerBuilder WithResponse(string path, HttpContent content, HttpStatusCode statusCode)
	{
		var mockResponse = new MockHttpResponse
		{
			Path = path,
			HttpStatusCode = statusCode,
			Content = content
		};

		if (!Responses.ContainsKey(path))
		{
			Responses[path] = new Queue<MockHttpResponse>();
		}

		Responses[path].Enqueue(mockResponse);

		return this;
	}
}
