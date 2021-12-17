using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

public interface IMessageAdapter
{
	string MessageIdentifier { get; }
}

public interface IMessageAdapter<out TMessage> : IMessageAdapter
{
	Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<TMessage> handler);
}
