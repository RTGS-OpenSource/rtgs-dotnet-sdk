using System.Net.Http;

namespace RTGS.DotNetSDK.IntegrationTests.HttpHandlers;

internal class QueueableStatusCodeHttpHandler : DelegatingHandler
{
	private readonly Dictionary<string, Queue<MockHttpResponse>> _mockHttpResponses;

	public Dictionary<string, IList<HttpRequestMessage>> Requests { get; } = new();

	public QueueableStatusCodeHttpHandler(Dictionary<string, Queue<MockHttpResponse>> mockHttpResponses)
	{
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

		var responseMock = _mockHttpResponses[requestPath].Dequeue();

		var response = new HttpResponseMessage(responseMock.HttpStatusCode);

		if (responseMock.Content is not null)
		{
			response.Content = new StringContent(responseMock.Content);
		}

		response.RequestMessage = request;

		return Task.FromResult(response);
	}
}
