using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestHandlers;

public static class HandlerCollectionExtensions
{
	public static IEnumerable<IHandler> ThrowWhenEarmarkCompleteV1Received(this IEnumerable<IHandler> handlers, Exception exceptionToThrow) =>
		handlers.Select(testHandler => testHandler is IEarmarkCompleteV1Handler
			? new EarmarkCompleteV1ThrowExceptionHandler(exceptionToThrow)
			: testHandler);
}
