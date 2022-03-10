extern alias RTGSServer;
using RTGSServer::RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.IntegrationTests.TestServer;

public class TestPaymentService : Payment.PaymentBase
{
	private readonly FromRtgsSender _fromRtgsSender;
	private readonly ToRtgsReceiver _receiver;
	private readonly ToRtgsMessageHandler _messageHandler;

	public TestPaymentService(FromRtgsSender fromRtgsSender, ToRtgsReceiver receiver, ToRtgsMessageHandler messageHandler)
	{
		_fromRtgsSender = fromRtgsSender;
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
