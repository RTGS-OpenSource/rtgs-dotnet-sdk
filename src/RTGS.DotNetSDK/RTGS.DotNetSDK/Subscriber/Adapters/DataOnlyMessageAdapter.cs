using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class DataOnlyMessageAdapter<TMessage> : IMessageAdapter<TMessage>
{
	public string MessageIdentifier => typeof(TMessage).Name;

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<TMessage> handler)
	{
		var deserializedMessage = JsonSerializer.Deserialize<TMessage>(rtgsMessage.Data);
		await handler.HandleMessageAsync(deserializedMessage);
	}
}
