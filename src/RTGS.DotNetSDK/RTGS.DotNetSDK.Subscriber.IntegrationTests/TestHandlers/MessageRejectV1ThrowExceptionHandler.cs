using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.ISO20022.Messages.Admi_002_001.V01;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers;

public class MessageRejectV1ThrowExceptionHandler : IMessageRejectV1Handler
{
	private readonly Exception _exception;

	public MessageRejectV1ThrowExceptionHandler(Exception exception)
	{
		_exception = exception;

	}

	public Task HandleMessageAsync(Admi00200101 message) =>
		throw _exception;
}
