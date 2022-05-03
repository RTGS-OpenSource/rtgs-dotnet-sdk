using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Messages.Subscriber;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class AtomicLockResponseV1MessageAdapter : IMessageAdapter<AtomicLockResponseV1>
{
	public string MessageIdentifier => "payment.lock.v2";

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<AtomicLockResponseV1> handler)
	{
		var atomicLockResponseMessage = JsonSerializer.Deserialize<AtomicLockResponseV1>(rtgsMessage.Data);
		await handler.HandleMessageAsync(atomicLockResponseMessage);
	}
}
