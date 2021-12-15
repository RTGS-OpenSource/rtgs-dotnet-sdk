using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers
{
	public static class HandlerCollectionExtensions
	{
		public static IEnumerable<IHandler> ThrowWhenMessageRejectV1Received(this IEnumerable<IHandler> handlers, Exception exceptionToThrow) =>
			handlers.Select(testHandler => testHandler is IMessageRejectV1Handler
				? new MessageRejectV1ThrowExceptionHandler(exceptionToThrow)
				: testHandler);
	}
}
