using System.Net;

namespace RTGS.DotNetSDK.IntegrationTests.HttpHandlers;

internal class StatusCodeHttpHandlerBuilderFactory
{
	public static StatusCodeHttpHandlerBuilder Create() => new();

	internal class StatusCodeHttpHandlerBuilder
	{
		private Dictionary<string, MockHttpResponse> Responses { get; } = new();

		public StatusCodeHttpHandlerBuilder WithServiceUnavailableResponse(string path) =>
			WithResponse(path, null, HttpStatusCode.ServiceUnavailable);

		public StatusCodeHttpHandlerBuilder WithOkResponse(HttpRequestResponseContext httpRequestResponseContext) =>
			WithResponse(
				httpRequestResponseContext.RequestPath,
				httpRequestResponseContext.ResponseContent,
				HttpStatusCode.OK);

		public StatusCodeHttpHandler Build() => new(Responses);

		private StatusCodeHttpHandlerBuilder WithResponse(string path, string content, HttpStatusCode statusCode)
		{
			var mockResponse = new MockHttpResponse
			{
				HttpStatusCode = statusCode,
				Content = content
			};

			Responses[path] = mockResponse;

			return this;
		}
	}
}
