using System.Text.Json;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters
{
	internal class EarmarkReleaseV1MessageAdapter : IMessageAdapter<EarmarkReleaseV1>
	{
		public string MessageIdentifier => "EarmarkRelease";

		public async Task HandleMessageAsync(RtgsMessage message, IHandler<EarmarkReleaseV1> handler)
		{
			var earmarkRelease = JsonSerializer.Deserialize<EarmarkReleaseV1>(message.Data);
			await handler.HandleMessageAsync(earmarkRelease);
		}
	}
}
