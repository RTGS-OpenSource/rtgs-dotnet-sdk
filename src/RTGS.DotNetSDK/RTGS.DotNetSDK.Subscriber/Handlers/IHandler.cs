namespace RTGS.DotNetSDK.Subscriber.Handlers;

public interface IHandler { }

public interface IHandler<in TMessage> : IHandler
{
	Task HandleMessageAsync(TMessage message);
}
