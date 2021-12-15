namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public class PublisherActionWithMessageIdentifier<TRequest> : IPublisherAction<TRequest>
	{
		private readonly IPublisherAction<TRequest> _publisherAction;

		public PublisherActionWithMessageIdentifier(IPublisherAction<TRequest> publisherAction, string messageIdentifier)
		{
			_publisherAction = publisherAction;
			MessageIdentifier = messageIdentifier;
		}

		public TRequest Request => _publisherAction.Request;

		public Task<SendResult> InvokeSendDelegateAsync(IRtgsPublisher publisher, CancellationToken cancellationToken = default) =>
			_publisherAction.InvokeSendDelegateAsync(publisher, cancellationToken);

		public string MessageIdentifier { get; }
	}
}
