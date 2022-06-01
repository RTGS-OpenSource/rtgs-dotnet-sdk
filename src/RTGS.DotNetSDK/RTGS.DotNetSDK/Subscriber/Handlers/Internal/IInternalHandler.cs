
namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;
internal interface IInternalHandler : IHandler
{
}
internal interface IInternalHandler<in TReceivedMessage> : IInternalHandler, IHandler<TReceivedMessage>
{
}
