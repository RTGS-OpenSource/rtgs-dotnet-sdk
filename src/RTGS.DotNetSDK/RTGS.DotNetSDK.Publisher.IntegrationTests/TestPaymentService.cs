extern alias RTGSServer;
using Grpc.Core;
using RTGSServer::RTGS.Public.Payment.V2;
using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class TestPaymentService : Payment.PaymentBase
	{
		private readonly IToRtgsReceiver _receiver;

		public TestPaymentService(IToRtgsReceiver receiver)
		{
			_receiver = receiver;
		}

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
				_receiver.AddRequest(message);

				await responseStream.WriteAsync(new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.OK,
					Success = true,
					Header = message.Header
				});
			}
		}
	}
}
