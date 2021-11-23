extern alias RTGSServer;
using System.Threading.Tasks;
using Grpc.Core;
using RTGSServer::RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer
{
	public class TestPaymentService : Payment.PaymentBase
	{
		private readonly ToRtgsReceiver _receiver;
		private readonly ToRtgsMessageHandler _messageHandler;

		public TestPaymentService(ToRtgsReceiver receiver, ToRtgsMessageHandler messageHandler)
		{
			_receiver = receiver;
			_messageHandler = messageHandler;
		}

		public override async Task ToRtgsMessage(
			IAsyncStreamReader<RtgsMessage> requestStream,
			IServerStreamWriter<RtgsMessageAcknowledgement> responseStream,
			ServerCallContext context)
		{
			var connectionInfo = _receiver.SetupConnectionInfo(context.RequestHeaders);

			await foreach (var message in requestStream.ReadAllAsync(context.CancellationToken))
			{
				await _messageHandler.Handle(message, responseStream);

				connectionInfo.Add(message);
			}
		}
	}
}
