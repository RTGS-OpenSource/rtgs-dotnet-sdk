using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Messages.Subscriber;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class PayawayFundsV1MessageAdapter : IMessageAdapter<PayawayFundsV1>
{
	public string MessageIdentifier => nameof(PayawayFundsV1);

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<PayawayFundsV1> handler)
	{
		var payawayFundsMessage = JsonSerializer.Deserialize<PayawayFundsV1>(rtgsMessage.Data);
		await handler.HandleMessageAsync(payawayFundsMessage);
	}
}
