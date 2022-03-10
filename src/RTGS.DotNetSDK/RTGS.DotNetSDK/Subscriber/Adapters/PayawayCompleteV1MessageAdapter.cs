using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class PayawayCompleteV1MessageAdapter : IMessageAdapter<BankToCustomerDebitCreditNotificationV09>
{
	public string MessageIdentifier => "PayawayComplete";

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<BankToCustomerDebitCreditNotificationV09> handler)
	{
		var payawayCompleteMessage = JsonSerializer.Deserialize<BankToCustomerDebitCreditNotificationV09>(rtgsMessage.Data);
		await handler.HandleMessageAsync(payawayCompleteMessage);
	}
}
