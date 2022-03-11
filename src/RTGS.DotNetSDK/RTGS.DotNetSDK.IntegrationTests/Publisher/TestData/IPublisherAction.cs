namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public interface IPublisherAction<out TRequest>
{
	TRequest Request { get; }

	Task<SendResult> InvokeSendDelegateAsync(IRtgsPublisher publisher, CancellationToken cancellationToken = default);
}
