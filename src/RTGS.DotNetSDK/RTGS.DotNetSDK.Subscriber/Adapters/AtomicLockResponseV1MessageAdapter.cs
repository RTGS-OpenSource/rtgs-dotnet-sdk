using System.Threading.Tasks;
using Newtonsoft.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters
{
	internal class AtomicLockResponseV1MessageAdapter : IMessageAdapter<AtomicLockResponseV1>
	{
		public string InstructionType => "payment.lock.v2";

		public async Task HandleMessageAsync(RtgsMessage message, IHandler<AtomicLockResponseV1> handler)
		{
			var atomicLockResponseMessage = JsonConvert.DeserializeObject<AtomicLockResponseV1>(message.Data);
			await handler.HandleMessageAsync(atomicLockResponseMessage);
		}
	}
}
