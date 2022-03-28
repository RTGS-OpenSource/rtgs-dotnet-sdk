using System.Net.Http;
using IDCryptGlobal.Cloud.Agent.Identity;
using Microsoft.Extensions.Options;

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
			.AddHttpClient<IIdentityClient, IdentityClient>((httpClient, serviceProvider) =>
			{
				var identityOptions = serviceProvider.GetRequiredService<IOptions<IdentityConfig>>();
				var identityClient = new IdentityClient(httpClient, identityOptions);

				return identityClient;
			})
			.AddHttpMessageHandler<THandler>();

		return serviceCollection;
	}
}
