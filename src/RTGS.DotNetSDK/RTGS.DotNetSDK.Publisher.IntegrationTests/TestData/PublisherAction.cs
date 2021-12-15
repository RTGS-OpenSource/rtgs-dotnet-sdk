namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;

public class PublisherAction<TRequest> : IPublisherAction<TRequest>
{
	private readonly Func<IRtgsPublisher, TRequest, CancellationToken, Task<SendResult>> _sendDelegate;

	public PublisherAction(TRequest request, Func<IRtgsPublisher, TRequest, CancellationToken, Task<SendResult>> sendDelegate)
	{
		_sendDelegate = sendDelegate;
		Request = request;
	}

	public TRequest Request { get; }

	public Task<SendResult> InvokeSendDelegateAsync(IRtgsPublisher publisher, CancellationToken cancellationToken = default) =>
		_sendDelegate(publisher, Request, cancellationToken);
}
