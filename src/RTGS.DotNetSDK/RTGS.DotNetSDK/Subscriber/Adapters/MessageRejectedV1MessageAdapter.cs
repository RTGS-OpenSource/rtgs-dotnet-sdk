using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Messages.Subscriber;
using RTGS.Public.Payment.V4;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class MessageRejectedV1MessageAdapter : IMessageAdapter<MessageRejectV1>
{
	public string MessageIdentifier => nameof(MessageRejectV1);

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<MessageRejectV1> handler)
	{
		var messageRejectedMessage = JsonSerializer.Deserialize<MessageRejectV1>(rtgsMessage.Data.Span);
		await handler.HandleMessageAsync(messageRejectedMessage);
	}
}
