using System.Net;
using System.Net.Http;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.RtgsConnectionBrokerTests.HttpHandlers;

internal class StatusCodeHttpHandler : DelegatingHandler
{
	private HttpResponseMessage _response;

	public StatusCodeHttpHandler()
	{
		var responseJson = "{\"connection_id\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," +
						   "\"invitation\":{" +
						   "\"@ID\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," +
						   "\"@Type\":\"https://didcomm.org/my-family/1.0/my-message-type\"," +
						   "\"label\":\"Bob\"," +
						   "\"recipientKeys\":[" +
						   "\"H3C2AVvLMv6gmMNam3uVAjZpfkcJCwDwnZn6z3wXmqPV\"]," +
						   "\"serviceEndpoint\":\"http://192.168.56.101:8020\"}}";

		_response = new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(responseJson)
		};
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		_response.RequestMessage = request;

		return Task.FromResult(_response);
	}
}
