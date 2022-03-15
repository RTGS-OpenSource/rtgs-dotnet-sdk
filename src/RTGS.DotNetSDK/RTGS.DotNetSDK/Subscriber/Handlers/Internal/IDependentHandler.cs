namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;

internal interface IDependentHandler : IHandler { }

internal interface IDependentHandler<TMessage, TDependentMessage> : IDependentHandler, IHandler<TMessage>
{
	IHandler<TDependentMessage> UserHandler { get; set; }
}
