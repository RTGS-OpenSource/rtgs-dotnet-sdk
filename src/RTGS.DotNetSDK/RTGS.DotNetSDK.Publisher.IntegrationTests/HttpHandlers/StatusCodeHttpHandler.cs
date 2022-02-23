﻿using System.Net.Http;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.HttpHandlers;

internal class StatusCodeHttpHandler : DelegatingHandler
{
	private readonly IList<MockHttpResponse> _mockHttpResponses;

	public Dictionary<string, HttpRequestMessage> Requests { get; private set; }

	public StatusCodeHttpHandler(IList<MockHttpResponse> mockHttpResponses)
	{
		Requests = new Dictionary<string, HttpRequestMessage>();
		_mockHttpResponses = mockHttpResponses;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		Requests[request.RequestUri.LocalPath] = request;

		var requestMock = _mockHttpResponses.FirstOrDefault(i => i.Path == request.RequestUri.LocalPath);

		var response = new HttpResponseMessage(requestMock.HttpStatusCode)
		{
			Content = requestMock.Content
		};

		response.RequestMessage = request;

		return Task.FromResult(response);
	}
}
