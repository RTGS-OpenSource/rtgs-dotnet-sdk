using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Messages.Subscriber;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class BankPartnersResponseV1MessageAdapter : IMessageAdapter<BankPartnersResponseV1>
{
	public string MessageIdentifier => "bank.partners.v1";

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<BankPartnersResponseV1> handler)
	{
		var bankPartnersResponseMessage = JsonSerializer.Deserialize<BankPartnersResponseV1>(rtgsMessage.Data);
		await handler.HandleMessageAsync(bankPartnersResponseMessage);
	}
}
