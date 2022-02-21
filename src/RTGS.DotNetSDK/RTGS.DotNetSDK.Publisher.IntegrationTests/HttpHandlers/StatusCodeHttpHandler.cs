using System.Net;
using System.Net.Http;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.HttpHandlers;

internal class StatusCodeHttpHandler : DelegatingHandler
{
	private readonly HttpStatusCode _statusCode;
	private readonly Dictionary<string, HttpContent> _contents;

	public Dictionary<string, HttpRequestMessage> Requests { get; private set; }

	public StatusCodeHttpHandler(HttpStatusCode statusCode, Dictionary<string, HttpContent> contents)
	{
		Requests = new Dictionary<string, HttpRequestMessage>();
		_statusCode = statusCode;
		_contents = contents;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		Requests[request.RequestUri.LocalPath] = request;

		var response = new HttpResponseMessage(_statusCode)
		{
			Content = _contents[request.RequestUri.LocalPath]
		};

		response.RequestMessage = request;

		return Task.FromResult(response);
	}
}
