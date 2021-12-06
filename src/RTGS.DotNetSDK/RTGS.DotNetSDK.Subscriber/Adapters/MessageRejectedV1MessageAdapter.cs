using System.Text.Json;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters
{
	public class MessageRejectedV1MessageAdapter : IMessageAdapter<Admi00200101>
	{
		public string InstructionType => "MessageRejected";

		public async Task HandleMessageAsync(RtgsMessage message, IHandler<Admi00200101> handler)
		{
			var payawayCompleteMessage = JsonSerializer.Deserialize<Admi00200101>(message.Data);
			await handler.HandleMessageAsync(payawayCompleteMessage);
		}
	}
}
