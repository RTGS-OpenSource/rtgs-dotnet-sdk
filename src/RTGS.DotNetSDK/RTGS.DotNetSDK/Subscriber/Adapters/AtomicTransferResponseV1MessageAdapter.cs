using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Messages.Subscriber;
using RTGS.Public.Payment.V4;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class AtomicTransferResponseV1MessageAdapter : IMessageAdapter<AtomicTransferResponseV1>
{
	public string MessageIdentifier => "payment.block.v2";

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<AtomicTransferResponseV1> handler)
	{
		var atomicTransferResponseMessage = JsonSerializer.Deserialize<AtomicTransferResponseV1>(rtgsMessage.Data.Span);
		await handler.HandleMessageAsync(atomicTransferResponseMessage);
	}
}
