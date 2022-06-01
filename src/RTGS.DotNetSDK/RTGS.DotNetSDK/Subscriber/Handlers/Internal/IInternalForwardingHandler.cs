namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;

internal interface IInternalForwardingHandler<in TReceivedMessage, out TUserHandledMessage> : IInternalHandler, IHandler<TReceivedMessage>
{
	void SetUserHandler(IHandler<TUserHandledMessage> userHandler);
}
