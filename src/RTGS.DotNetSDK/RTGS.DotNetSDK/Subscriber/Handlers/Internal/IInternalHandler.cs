namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;

internal interface IInternalHandler : IHandler { }

internal interface IInternalHandler<TReceivedMessage, TUserHandledMessage> : IInternalHandler, IHandler<TReceivedMessage>
{
	void SetUserHandler(IHandler<TUserHandledMessage> userHandler);
}
