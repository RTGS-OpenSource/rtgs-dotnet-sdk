using System.Threading.Tasks;
using Newtonsoft.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters
{
	internal class AtomicTransferResponseV1MessageAdapter : IMessageAdapter<AtomicTransferResponseV1>
	{
		public string InstructionType => "BlockResponse";

		public async Task HandleMessageAsync(RtgsMessage message, IHandler<AtomicTransferResponseV1> handler)
		{
			var atomicTransferResponseMessage = JsonConvert.DeserializeObject<AtomicTransferResponseV1>(message.Data);
			await handler.HandleMessageAsync(atomicTransferResponseMessage);
		}
	}
}
