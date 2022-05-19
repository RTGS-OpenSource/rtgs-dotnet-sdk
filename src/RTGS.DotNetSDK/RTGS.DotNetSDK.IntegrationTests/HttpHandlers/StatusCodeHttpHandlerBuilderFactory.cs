using System.Net;

namespace RTGS.DotNetSDK.IntegrationTests.HttpHandlers;

internal class StatusCodeHttpHandlerBuilderFactory
{
	public static StatusCodeHttpHandlerBuilder Create() => new();
	public static QueueableStatusCodeHttpHandlerBuilder CreateQueueable() => new();

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

	internal class QueueableStatusCodeHttpHandlerBuilder
	{
		private Dictionary<string, Queue<MockHttpResponse>> Responses { get; } = new();

		public QueueableStatusCodeHttpHandlerBuilder WithServiceUnavailableResponse(string path) =>
			WithResponse(path, null, HttpStatusCode.ServiceUnavailable);

		public QueueableStatusCodeHttpHandlerBuilder WithOkResponse(HttpRequestResponseContext httpRequestResponseContext) =>
			WithResponse(
				httpRequestResponseContext.RequestPath,
				httpRequestResponseContext.ResponseContent,
				HttpStatusCode.OK);

		public QueueableStatusCodeHttpHandler Build() => new(Responses);

		private QueueableStatusCodeHttpHandlerBuilder WithResponse(string path, string content, HttpStatusCode statusCode)
		{
			var mockResponse = new MockHttpResponse
			{
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
}
