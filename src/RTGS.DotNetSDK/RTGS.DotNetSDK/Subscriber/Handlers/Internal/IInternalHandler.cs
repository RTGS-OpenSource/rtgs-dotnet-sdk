
namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;
internal interface IInternalHandler : IHandler
{
}
internal interface IInternalHandler<in TReceivedMessage> : IInternalHandler, IHandler<TReceivedMessage>
{
}
internal interface IInternalPassThroughHandler<in TReceivedMessage, out TUserHandledMessage> : IInternalHandler, IHandler<TReceivedMessage>
{
	void SetUserHandler(IHandler<TUserHandledMessage> userHandler);
}
