using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters
{
	internal class MessageRejectedV1MessageAdapter : IMessageAdapter<Admi00200101>
	{
		public string MessageIdentifier => "MessageRejected";

		public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<Admi00200101> handler)
		{
			var messageRejectedMessage = JsonSerializer.Deserialize<Admi00200101>(rtgsMessage.Data);
			await handler.HandleMessageAsync(messageRejectedMessage);
		}
	}
}
