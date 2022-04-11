using System.Net.Http;

namespace RTGS.DotNetSDK.IntegrationTests.Extensions;

// ReSharper disable once InconsistentNaming

internal static class IServiceCollectionExtensions
{
	public static IServiceCollection AddTestIdCryptHttpClient<THandler>(
		this IServiceCollection serviceCollection,
		THandler statusCodeHttpHandler) where THandler : DelegatingHandler
	{
		serviceCollection
			.AddSingleton(statusCodeHttpHandler)
			.AddHttpClient("AgentHttpClient")
			.AddHttpMessageHandler<THandler>();

		return serviceCollection;
	}
}
