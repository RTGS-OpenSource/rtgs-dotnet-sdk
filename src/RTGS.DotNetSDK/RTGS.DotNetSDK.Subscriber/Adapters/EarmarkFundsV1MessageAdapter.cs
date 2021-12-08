using System.Text.Json;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters
{
	internal class EarmarkFundsV1MessageAdapter : IMessageAdapter<EarmarkFundsV1>
	{
		public string MessageIdentifier => "EarmarkFunds";

		public async Task HandleMessageAsync(RtgsMessage message, IHandler<EarmarkFundsV1> handler)
		{
			var earmarkFundsMessage = JsonSerializer.Deserialize<EarmarkFundsV1>(message.Data);
			await handler.HandleMessageAsync(earmarkFundsMessage);
		}
	}
}
