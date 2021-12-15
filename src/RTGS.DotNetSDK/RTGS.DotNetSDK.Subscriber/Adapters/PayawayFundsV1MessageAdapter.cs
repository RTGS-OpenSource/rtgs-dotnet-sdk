using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters
{
	internal class PayawayFundsV1MessageAdapter : IMessageAdapter<FIToFICustomerCreditTransferV10>
	{
		public string MessageIdentifier => "PayawayFunds";

		public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<FIToFICustomerCreditTransferV10> handler)
		{
			var payawayFundsMessage = JsonSerializer.Deserialize<FIToFICustomerCreditTransferV10>(rtgsMessage.Data);
			await handler.HandleMessageAsync(payawayFundsMessage);
		}
	}
}
