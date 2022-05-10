using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Messages.Subscriber;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class PayawayCompleteV1MessageAdapter : IMessageAdapter<PayawayCompleteV1>
{
	public string MessageIdentifier => nameof(PayawayCompleteV1);

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<PayawayCompleteV1> handler)
	{
		var payawayCompleteMessage = JsonSerializer.Deserialize<PayawayCompleteV1>(rtgsMessage.Data);
		await handler.HandleMessageAsync(payawayCompleteMessage);
	}
}
