using System.Text.Json;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters
{
	internal class EarmarkCompleteV1MessageAdapter : IMessageAdapter<EarmarkCompleteV1>
	{
		public string MessageIdentifier => "EarmarkComplete";

		public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<EarmarkCompleteV1> handler)
		{
			var earmarkCompleteMessage = JsonSerializer.Deserialize<EarmarkCompleteV1>(rtgsMessage.Data);
			await handler.HandleMessageAsync(earmarkCompleteMessage);
		}
	}
}
