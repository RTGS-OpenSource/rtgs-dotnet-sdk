namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public interface IPublisherAction<out TRequest>
	{
		TRequest Request { get; }

		Task<SendResult> InvokeSendDelegateAsync(IRtgsPublisher publisher, CancellationToken cancellationToken = default);
	}
}
