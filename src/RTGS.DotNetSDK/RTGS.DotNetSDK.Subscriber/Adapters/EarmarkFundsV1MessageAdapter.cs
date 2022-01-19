using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class EarmarkFundsV1MessageAdapter : IMessageAdapter<EarmarkFundsV1>
{
	public string MessageIdentifier => "EarmarkFunds";

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<EarmarkFundsV1> handler)
	{
		var earmarkFundsMessage = JsonSerializer.Deserialize<EarmarkFundsV1>(rtgsMessage.Data);
		await handler.HandleMessageAsync(earmarkFundsMessage);
	}
}
