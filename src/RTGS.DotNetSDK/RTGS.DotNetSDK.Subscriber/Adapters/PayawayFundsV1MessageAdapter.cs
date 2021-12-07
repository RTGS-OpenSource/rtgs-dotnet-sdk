using System.Text.Json;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters
{
	internal class PayawayFundsV1MessageAdapter : IMessageAdapter<FIToFICustomerCreditTransferV10>
	{
		public string InstructionType => "PayawayFunds";

		public async Task HandleMessageAsync(RtgsMessage message, IHandler<FIToFICustomerCreditTransferV10> handler)
		{
			var payawayFundsMessage = JsonSerializer.Deserialize<FIToFICustomerCreditTransferV10>(message.Data);
			await handler.HandleMessageAsync(payawayFundsMessage);
		}
	}
}
