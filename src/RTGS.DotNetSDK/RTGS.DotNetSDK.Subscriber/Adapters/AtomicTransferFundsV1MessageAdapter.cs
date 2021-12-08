using System.Threading.Tasks;
using Newtonsoft.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters
{
	internal class AtomicTransferFundsV1MessageAdapter : IMessageAdapter<AtomicTransferFundsV1>
	{
		public string MessageIdentifier => "BlockFunds";

		public async Task HandleMessageAsync(RtgsMessage message, IHandler<AtomicTransferFundsV1> handler)
		{
			var atomicTransferFundsMessage = JsonConvert.DeserializeObject<AtomicTransferFundsV1>(message.Data);
			await handler.HandleMessageAsync(atomicTransferFundsMessage);
		}
	}
}
