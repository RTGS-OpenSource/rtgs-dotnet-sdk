extern alias RTGSServer;
using RTGSServer::RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestServer
{
	public class TestPaymentService : Payment.PaymentBase
	{
		private readonly FromRtgsSender _fromRtgsSender;

		public TestPaymentService(FromRtgsSender fromRtgsSender)
		{
			_fromRtgsSender = fromRtgsSender;
		}

		public override async Task FromRtgsMessage(IAsyncStreamReader<RtgsMessageAcknowledgement> requestStream, IServerStreamWriter<RtgsMessage> responseStream, ServerCallContext context)
		{
			try
			{
				_fromRtgsSender.Register(responseStream, context.RequestHeaders);

				await foreach (var message in requestStream.ReadAllAsync(context.CancellationToken))
				{
					_fromRtgsSender.AddAcknowledgement(message);
				}
			}
			finally
			{
				_fromRtgsSender.Unregister();
			}
		}
	}
}
