using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Messages.Subscriber;
using RTGS.Public.Payment.V4;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class AtomicTransferFundsV1MessageAdapter : IMessageAdapter<AtomicTransferFundsV1>
{
	public string MessageIdentifier => "payment.blockfunds.v1";

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<AtomicTransferFundsV1> handler)
	{
		var atomicTransferFundsMessage = JsonSerializer.Deserialize<AtomicTransferFundsV1>(rtgsMessage.Data.Span);
		await handler.HandleMessageAsync(atomicTransferFundsMessage);
	}
}
