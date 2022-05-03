using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.IntegrationTests.Extensions;

public static class HandlerExtensions
{
	public static AllTestHandlers.TestHandler<T> GetHandler<T>(this IList<IHandler> handlers) =>
		handlers.OfType<AllTestHandlers.TestHandler<T>>().Single();
}
