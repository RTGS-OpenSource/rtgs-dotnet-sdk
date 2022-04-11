using System.Text.Json;
using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class IdCryptBankInvitationV1MessageAdapter : IMessageAdapter<IdCryptBankInvitationV1>
{
	public string MessageIdentifier => "idcrypt.invitation.tobank.v1";

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<IdCryptBankInvitationV1> handler)
	{
		var idCryptBankInvitation = JsonSerializer.Deserialize<IdCryptBankInvitationV1>(rtgsMessage.Data);
		await handler.HandleMessageAsync(idCryptBankInvitation);
	}
}
