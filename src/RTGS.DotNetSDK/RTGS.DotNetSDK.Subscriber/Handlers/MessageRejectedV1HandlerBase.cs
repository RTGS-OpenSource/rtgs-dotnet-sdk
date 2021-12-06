using System.Text.Json;
using System.Threading.Tasks;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Handlers
{
	public abstract class MessageRejectedV1HandlerBase : IHandler
	{
		public string InstructionType => "MessageRejected";

		public Task HandleMessageAsync(RtgsMessage message)
		{
			var messageRejectedMessage = JsonSerializer.Deserialize<Admi00200101>(message.Data);
			return HandleMessageAsync(messageRejectedMessage);
		}

		protected abstract Task HandleMessageAsync(Admi00200101 message);
	}
}
