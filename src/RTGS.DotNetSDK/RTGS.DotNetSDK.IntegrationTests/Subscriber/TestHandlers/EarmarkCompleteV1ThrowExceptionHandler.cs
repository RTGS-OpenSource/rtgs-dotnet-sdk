using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestHandlers;

public class EarmarkCompleteV1ThrowExceptionHandler : IEarmarkCompleteV1Handler
{
	private readonly Exception _exception;

	public EarmarkCompleteV1ThrowExceptionHandler(Exception exception)
	{
		_exception = exception;
	}

	public Task HandleMessageAsync(EarmarkCompleteV1 message) =>
		throw _exception;
}
