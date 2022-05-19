using System.Text.Json;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public class PublisherAction<TRequest> : IPublisherAction<TRequest>
{
	private readonly Func<IRtgsPublisher, TRequest, CancellationToken, Task<SendResult>> _sendDelegate;

	public PublisherAction(TRequest request, Func<IRtgsPublisher, TRequest, CancellationToken, Task<SendResult>> sendDelegate, object signedDocument = null)
		: this(request, new Dictionary<string, string>(), sendDelegate, signedDocument)
	{
	}

	public PublisherAction(
		TRequest request,
		Dictionary<string, string> headers,
		Func<IRtgsPublisher, TRequest, CancellationToken, Task<SendResult>> sendDelegate,
		object signedDocument = null)
	{
		_sendDelegate = sendDelegate;
		Request = request;
		Headers = headers;
		SerialisedSignedDocument = signedDocument == null
			? null
			: JsonSerializer.Serialize(new { rtgsGlobalId = ValidMessages.RtgsGlobalId, message = signedDocument });
	}

	public string SerialisedSignedDocument { get; }

	public TRequest Request { get; }

	public Dictionary<string, string> Headers { get; }

	public Task<SendResult> InvokeSendDelegateAsync(IRtgsPublisher publisher, CancellationToken cancellationToken = default) =>
		_sendDelegate(publisher, Request, cancellationToken);
}
