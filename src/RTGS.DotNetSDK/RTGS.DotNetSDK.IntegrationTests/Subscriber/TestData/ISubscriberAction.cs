namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public interface ISubscriberAction<out TMessage>
{
	string MessageIdentifier { get; }
	TMessage Message { get; }
}
