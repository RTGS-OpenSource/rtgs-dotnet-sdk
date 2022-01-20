namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;

public class PublisherAction<TRequest> : IPublisherAction<TRequest>
{
	private readonly Func<IRtgsPublisher, TRequest, CancellationToken, Task<SendResult>> _sendDelegate;

	public PublisherAction(TRequest request, Func<IRtgsPublisher, TRequest, CancellationToken, Task<SendResult>> sendDelegate) : this(request, new Dictionary<string, string>(), sendDelegate)
	{
	}

	public PublisherAction(TRequest request, Dictionary<string, string> headers, Func<IRtgsPublisher, TRequest, CancellationToken, Task<SendResult>> sendDelegate)
	{
		_sendDelegate = sendDelegate;
		Request = request;
		Headers = headers;
	}

	public TRequest Request { get; }

	public Dictionary<string, string> Headers { get; }

	public Task<SendResult> InvokeSendDelegateAsync(IRtgsPublisher publisher, CancellationToken cancellationToken = default) =>
		_sendDelegate(publisher, Request, cancellationToken);
}
