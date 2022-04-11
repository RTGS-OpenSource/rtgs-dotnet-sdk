﻿using System.Text.Json;
using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class IdCryptInvitationConfirmationV1MessageAdapter : IMessageAdapter<IdCryptInvitationConfirmationV1>
{
	public string MessageIdentifier => "idcrypt.invitationconfirmation.v1";

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<IdCryptInvitationConfirmationV1> handler)
	{
		var idCryptInvitationConfirmationMessage = JsonSerializer.Deserialize<IdCryptInvitationConfirmationV1>(rtgsMessage.Data);
		await handler.HandleMessageAsync(idCryptInvitationConfirmationMessage);
	}
}
