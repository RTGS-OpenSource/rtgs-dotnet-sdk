namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;

internal interface IInternalHandler : IHandler { }

internal interface IInternalHandler<in TReceivedMessage, out TUserHandledMessage> : IInternalHandler, IHandler<TReceivedMessage>
{
	void SetUserHandler(IHandler<TUserHandledMessage> userHandler);
}
