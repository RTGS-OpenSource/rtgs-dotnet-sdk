using IDCryptGlobal.Cloud.Agent.Identity;
using Microsoft.Extensions.Options;
using RTGS.DotNetSDK.Publisher.IntegrationTests.HttpHandlers;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.Extensions;

internal static class IServiceCollectionExtensions
{
	public static IServiceCollection AddTestIdCryptHttpClient(
		this IServiceCollection serviceCollection,
		StatusCodeHttpHandler statusCodeHttpHandler)
	{
		serviceCollection
			.AddSingleton(statusCodeHttpHandler)
			.AddHttpClient<IIdentityClient, IdentityClient>((httpClient, serviceProvider) =>
			{
				var identityOptions = serviceProvider.GetRequiredService<IOptions<IdentityConfig>>();
				var identityClient = new IdentityClient(httpClient, identityOptions);

				return identityClient;
			})
			.AddHttpMessageHandler<StatusCodeHttpHandler>();

		return serviceCollection;
	}
}
