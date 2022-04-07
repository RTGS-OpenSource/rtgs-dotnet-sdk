using System.Net.Http;

namespace RTGS.DotNetSDK.IntegrationTests.HttpHandlers;

internal class SecondaryStatusCodeHttpHandler : StatusCodeHttpHandler
{
	public SecondaryStatusCodeHttpHandler(Dictionary<string, MockHttpResponse> mockHttpResponses) : base(mockHttpResponses)
	{

	}
}

internal class StatusCodeHttpHandler : DelegatingHandler
{
	private readonly Dictionary<string, MockHttpResponse> _mockHttpResponses;

	public Dictionary<string, IList<HttpRequestMessage>> Requests { get; }

	public StatusCodeHttpHandler(Dictionary<string, MockHttpResponse> mockHttpResponses)
	{
		Requests = new Dictionary<string, IList<HttpRequestMessage>>();
		_mockHttpResponses = mockHttpResponses;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var requestPath = request.RequestUri!.LocalPath;

		if (!Requests.ContainsKey(requestPath))
		{
			Requests[requestPath] = new List<HttpRequestMessage>();
		}
		Requests[requestPath].Add(request);

		var responseMock = _mockHttpResponses[requestPath];

		var response = new HttpResponseMessage(responseMock.HttpStatusCode)
		{
			Content = new StringContent(responseMock.Content)
		};

		response.RequestMessage = request;

		return Task.FromResult(response);
	}
}
