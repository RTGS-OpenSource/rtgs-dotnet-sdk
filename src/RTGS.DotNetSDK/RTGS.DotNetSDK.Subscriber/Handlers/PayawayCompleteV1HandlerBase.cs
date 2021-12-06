using System.Text.Json;
using System.Threading.Tasks;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Handlers
{
	public abstract class PayawayCompleteV1HandlerBase : IHandler
	{
		public string InstructionType => "PayawayComplete";

		public Task HandleMessageAsync(RtgsMessage message)
		{
			var payawayCompleteMessage = JsonSerializer.Deserialize<BankToCustomerDebitCreditNotificationV09>(message.Data);
			return HandleMessageAsync(payawayCompleteMessage);
		}

		protected abstract Task HandleMessageAsync(BankToCustomerDebitCreditNotificationV09 message);
	}
}
