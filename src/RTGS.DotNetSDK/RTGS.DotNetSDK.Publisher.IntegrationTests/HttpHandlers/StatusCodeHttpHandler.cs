using System.Net;
using System.Net.Http;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.HttpHandlers;

internal class StatusCodeHttpHandler : DelegatingHandler
{
	private HttpResponseMessage _response;

    public HttpRequestMessage Request { get; private set; }

	public HttpStatusCode StatusCode
	{
		get
		{
			return _response.StatusCode;
		}

		set
		{
			_response.StatusCode = value;
		}
	}

	public HttpContent Content
	{
		get
		{
			return _response.Content;
		}

		set
		{
			_response.Content = value;
		}
	}


	public StatusCodeHttpHandler(HttpStatusCode statusCode, HttpContent content)
	{
		_response = new HttpResponseMessage(statusCode)
		{
			Content = content
		};
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Request = request;

		_response.RequestMessage = request;

		return Task.FromResult(_response);
	}
}
