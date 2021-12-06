using System.Text.Json;
using System.Threading.Tasks;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Handlers
{
	public abstract class PayawayFundsV1HandlerBase : IHandler
	{
		public string InstructionType => "PayawayFunds";

		public Task HandleMessageAsync(RtgsMessage message)
		{
			var payawayFundsMessage = JsonSerializer.Deserialize<FIToFICustomerCreditTransferV10>(message.Data);
			return HandleMessageAsync(payawayFundsMessage);
		}

		protected abstract Task HandleMessageAsync(FIToFICustomerCreditTransferV10 message);
	}
}
