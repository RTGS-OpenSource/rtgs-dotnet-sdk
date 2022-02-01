using System.Net;
using System.Net.Http;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.RtgsConnectionBrokerTests.HttpHandlers;

internal class StatusCodeHttpHandler : DelegatingHandler
{
	private HttpResponseMessage _response;

	public StatusCodeHttpHandler(HttpStatusCode statusCode, HttpContent content)
	{
		_response = new HttpResponseMessage(statusCode)
		{
			Content = content
		};
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		_response.RequestMessage = request;

		return Task.FromResult(_response);
	}
}
