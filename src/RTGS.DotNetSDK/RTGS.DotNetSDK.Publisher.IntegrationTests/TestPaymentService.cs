extern alias RTGSServer;
using System;
using System.Threading.Tasks;
using Grpc.Core;
using RTGSServer::RTGS.Public.Payment.V2;

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
			var x = context.RequestHeaders;

			try
			{
				await foreach (var message in requestStream.ReadAllAsync(context.CancellationToken))
				{
					_receiver.AddRequest(message);

					await responseStream.WriteAsync(new RtgsMessageAcknowledgement
					{
						Code = (int)StatusCode.OK,
						Success = true,
						Header = new RtgsMessageHeader()
					});
				}

				var z = "the end";
			}
			catch (Exception ex)
			{
				var m = ex.Message;
				throw;
			}
		}
	}
}
