using System.Text.Json;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters
{
	internal class EarmarkCompleteV1MessageAdapter : IMessageAdapter<EarmarkCompleteV1>
	{
		public string InstructionType => "EarmarkComplete";

		public async Task HandleMessageAsync(RtgsMessage message, IHandler<EarmarkCompleteV1> handler)
		{
			var earmarkComplete = JsonSerializer.Deserialize<EarmarkCompleteV1>(message.Data);
			await handler.HandleMessageAsync(earmarkComplete);
		}
	}
}
