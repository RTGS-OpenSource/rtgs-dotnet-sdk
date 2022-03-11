using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class AtomicTransferFundsV1MessageAdapter : IMessageAdapter<AtomicTransferFundsV1>
{
	public string MessageIdentifier => "payment.blockfunds.v1";

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<AtomicTransferFundsV1> handler)
	{
		var atomicTransferFundsMessage = JsonSerializer.Deserialize<AtomicTransferFundsV1>(rtgsMessage.Data);
		await handler.HandleMessageAsync(atomicTransferFundsMessage);
	}
}
