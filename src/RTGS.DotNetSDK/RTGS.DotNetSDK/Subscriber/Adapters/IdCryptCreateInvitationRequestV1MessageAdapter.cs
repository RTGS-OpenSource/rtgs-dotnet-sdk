using System.Text.Json;
using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class IdCryptCreateInvitationRequestV1MessageAdapter : IMessageAdapter<IdCryptCreateInvitationRequestV1>
{
	public string MessageIdentifier => "idcrypt.createinvitation.v1";

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<IdCryptCreateInvitationRequestV1> handler)
	{
		var idCryptCreateInvitationRequestMessage = JsonSerializer.Deserialize<IdCryptCreateInvitationRequestV1>(rtgsMessage.Data);
		await handler.HandleMessageAsync(idCryptCreateInvitationRequestMessage);
	}
}
