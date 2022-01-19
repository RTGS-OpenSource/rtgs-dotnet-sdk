using Newtonsoft.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class AtomicTransferResponseV1MessageAdapter : IMessageAdapter<AtomicTransferResponseV1>
{
	public string MessageIdentifier => "BlockResponse";

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<AtomicTransferResponseV1> handler)
	{
		var atomicTransferResponseMessage = JsonConvert.DeserializeObject<AtomicTransferResponseV1>(rtgsMessage.Data);
		await handler.HandleMessageAsync(atomicTransferResponseMessage);
	}
}
