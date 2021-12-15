using System.Text.Json;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters;

internal class EarmarkReleaseV1MessageAdapter : IMessageAdapter<EarmarkReleaseV1>
{
	public string MessageIdentifier => "EarmarkRelease";

	public async Task HandleMessageAsync(RtgsMessage rtgsMessage, IHandler<EarmarkReleaseV1> handler)
	{
		var earmarkReleaseMessage = JsonSerializer.Deserialize<EarmarkReleaseV1>(rtgsMessage.Data);
		await handler.HandleMessageAsync(earmarkReleaseMessage);
	}
}
