namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public interface IVerifiableSubscriberAction<out TMessage, out TVerifyMessage> : ISubscriberAction<TMessage>
{
	TVerifyMessage VerifyMessage { get; }
}
