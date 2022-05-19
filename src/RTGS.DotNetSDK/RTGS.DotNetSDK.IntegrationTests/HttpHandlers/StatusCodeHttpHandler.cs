using System.Net.Http;

namespace RTGS.DotNetSDK.IntegrationTests.HttpHandlers;

// TODO JLIQ - Change implementation to old queueable handler
internal class StatusCodeHttpHandler : DelegatingHandler
{
	private readonly Dictionary<string, MockHttpResponse> _mockHttpResponses;
	private readonly CountdownEvent _requestsSignal;

	public Dictionary<string, IList<HttpRequestMessage>> Requests { get; } = new();

	public StatusCodeHttpHandler(Dictionary<string, MockHttpResponse> mockHttpResponses)
	{
		_mockHttpResponses = mockHttpResponses;

		_requestsSignal = new CountdownEvent(_mockHttpResponses.Count);
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

		var response = new HttpResponseMessage(responseMock.HttpStatusCode);

		if (responseMock.Content is not null)
		{
			response.Content = new StringContent(responseMock.Content);
		}

		response.RequestMessage = request;

		_requestsSignal.Signal();

		return Task.FromResult(response);
	}

	public void WaitForRequests(TimeSpan timeout) => _requestsSignal.Wait(timeout);

	public void Reset()
	{
		Requests.Clear();
		_requestsSignal.Reset();
	}
}
