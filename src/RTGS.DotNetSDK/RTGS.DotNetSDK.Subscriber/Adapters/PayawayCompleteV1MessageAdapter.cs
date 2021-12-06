using System.Text.Json;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters
{
	public class PayawayCompleteV1MessageAdapter : IMessageAdapter<BankToCustomerDebitCreditNotificationV09>
	{
		public string InstructionType => "PayawayComplete";

		public async Task HandleMessageAsync(RtgsMessage message, IHandler<BankToCustomerDebitCreditNotificationV09> handler)
		{
			var payawayCompleteMessage = JsonSerializer.Deserialize<BankToCustomerDebitCreditNotificationV09>(message.Data);
			await handler.HandleMessageAsync(payawayCompleteMessage);
		}
	}
}
