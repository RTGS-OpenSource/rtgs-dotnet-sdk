﻿using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Messages.Subscriber;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class EarmarkCompleteV1MessageAdapter : IMessageAdapter<EarmarkCompleteV1>
{
	public string MessageIdentifier => "EarmarkComplete";

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<EarmarkCompleteV1> handler)
	{
		var earmarkCompleteMessage = JsonSerializer.Deserialize<EarmarkCompleteV1>(rtgsMessage.Data);
		await handler.HandleMessageAsync(earmarkCompleteMessage);
	}
}
