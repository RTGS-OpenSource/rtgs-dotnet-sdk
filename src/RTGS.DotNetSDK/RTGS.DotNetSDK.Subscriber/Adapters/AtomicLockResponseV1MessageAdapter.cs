using Newtonsoft.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters
{
	internal class AtomicLockResponseV1MessageAdapter : IMessageAdapter<AtomicLockResponseV1>
	{
		public string MessageIdentifier => "payment.lock.v2";

		public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<AtomicLockResponseV1> handler)
		{
			var atomicLockResponseMessage = JsonConvert.DeserializeObject<AtomicLockResponseV1>(rtgsMessage.Data);
			await handler.HandleMessageAsync(atomicLockResponseMessage);
		}
	}
}
