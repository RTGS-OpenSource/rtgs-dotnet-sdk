using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Messages.Subscriber;
using RTGS.Public.Payment.V4;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class EarmarkFundsV1MessageAdapter : IMessageAdapter<EarmarkFundsV1>
{
	public string MessageIdentifier => "EarmarkFunds";

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<EarmarkFundsV1> handler)
	{
		var earmarkFundsMessage = JsonSerializer.Deserialize<EarmarkFundsV1>(rtgsMessage.Data.Span);
		await handler.HandleMessageAsync(earmarkFundsMessage);
	}
}
