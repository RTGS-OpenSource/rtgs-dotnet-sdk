using System.Threading.Tasks;
using Newtonsoft.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters
{
	internal class AtomicTransferFundsV1MessageAdapter : IMessageAdapter<BlockFundsV1>
	{
		public string MessageIdentifier => "BlockFunds";

		public async Task HandleMessageAsync(RtgsMessage message, IHandler<BlockFundsV1> handler)
		{
			var blockFundsMessage = JsonConvert.DeserializeObject<BlockFundsV1>(message.Data);
			await handler.HandleMessageAsync(blockFundsMessage);
		}
	}
}
