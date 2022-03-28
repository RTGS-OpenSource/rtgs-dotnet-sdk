using System.Net.Http;

namespace RTGS.DotNetSDK.IntegrationTests.HttpHandlers;

internal class QueueableStatusCodeHttpHandler : DelegatingHandler
{
	private readonly Dictionary<string, Queue<MockHttpResponse>> _mockHttpResponses;

	public Dictionary<string, IList<HttpRequestMessage>> Requests { get; }

	public QueueableStatusCodeHttpHandler(Dictionary<string, Queue<MockHttpResponse>> mockHttpResponses)
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

		var requestMock = _mockHttpResponses[requestPath].Dequeue();

		var response = new HttpResponseMessage(requestMock.HttpStatusCode)
		{
			Content = requestMock.Content
		};

		response.RequestMessage = request;

		return Task.FromResult(response);
	}
}
