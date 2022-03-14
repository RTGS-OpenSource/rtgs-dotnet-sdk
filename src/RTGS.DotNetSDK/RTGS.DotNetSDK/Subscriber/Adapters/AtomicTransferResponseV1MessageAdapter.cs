﻿using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class AtomicTransferResponseV1MessageAdapter : IMessageAdapter<AtomicTransferResponseV1>
{
	public string MessageIdentifier => "payment.block.v2";

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<AtomicTransferResponseV1> handler)
	{
		var atomicTransferResponseMessage = JsonSerializer.Deserialize<AtomicTransferResponseV1>(rtgsMessage.Data);
		await handler.HandleMessageAsync(atomicTransferResponseMessage);
	}
}