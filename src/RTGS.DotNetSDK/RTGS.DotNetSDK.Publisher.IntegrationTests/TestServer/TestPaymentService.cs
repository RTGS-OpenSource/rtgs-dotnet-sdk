﻿extern alias RTGSServer;
using Grpc.Core;
using RTGSServer::RTGS.Public.Payment.V2;
using System.Threading.Tasks;

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
			var handledMessages = _receiver.SetupConnectionInfo(context.RequestHeaders);

			await foreach (var message in requestStream.ReadAllAsync(context.CancellationToken))
			{
				await _messageHandler.Handle(message, responseStream);

				handledMessages.Add(message);
			}
		}
	}
}
