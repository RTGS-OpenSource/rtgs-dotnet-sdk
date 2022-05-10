using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestHandlers;

public class MessageRejectV1ThrowExceptionHandler : IMessageRejectV1Handler
{
	private readonly Exception _exception;

	public MessageRejectV1ThrowExceptionHandler(Exception exception)
	{
		_exception = exception;

	}

	public Task HandleMessageAsync(MessageRejectV1 message) =>
		throw _exception;
}
