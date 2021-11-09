extern alias RTGSServer;
using System.Threading.Tasks;
using Grpc.Core;
using RTGSServer::RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class TestPaymentService : Payment.PaymentBase
	{
		public override async Task FromRtgsMessage(IAsyncStreamReader<RtgsMessageAcknowledgement> requestStream, IServerStreamWriter<RtgsMessage> responseStream, ServerCallContext context)
		{
			await foreach (var message in requestStream.ReadAllAsync(context.CancellationToken))
			{
				// TODO: code here
			}
		}

		public override async Task ToRtgsMessage(IAsyncStreamReader<RtgsMessage> requestStream, IServerStreamWriter<RtgsMessageAcknowledgement> responseStream, ServerCallContext context)
		{
			await foreach (var message in requestStream.ReadAllAsync(context.CancellationToken))
			{
				// TODO: code here
			}
		}
	}
}
