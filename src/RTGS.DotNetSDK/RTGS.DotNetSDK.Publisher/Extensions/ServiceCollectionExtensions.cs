using System.Net.Http;
using IDCryptGlobal.Cloud.Agent.Identity;
using Microsoft.Extensions.DependencyInjection;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Publisher.Extensions;

/// <summary>
/// ServiceCollection Extensions class
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds <seealso cref="IRtgsPublisher"/> with supplied client configuration of <seealso cref="RtgsPublisherOptions"/>.
	/// </summary>
	/// <param name="serviceCollection">The service collection</param>
	/// <param name="publisherOptions">The options used to build the publisher</param>
	/// <param name="configureGrpcClient">The client configure action (optional)</param>
	/// <returns>The service collection so that additional calls can be chained.</returns>
	public static IServiceCollection AddRtgsPublisher(
		this IServiceCollection serviceCollection,
		RtgsPublisherOptions publisherOptions,
		Action<IHttpClientBuilder> configureGrpcClient = null)
	{
		serviceCollection.AddSingleton(publisherOptions);

		var grpcClientBuilder = serviceCollection
			.AddGrpcClient<Payment.PaymentClient>(clientOptions => clientOptions.Address = publisherOptions.RemoteHostAddress)
			.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
			{
				PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
				KeepAlivePingDelay = publisherOptions.KeepAlivePingDelay,
				KeepAlivePingTimeout = publisherOptions.KeepAlivePingTimeout,
				EnableMultipleHttp2Connections = true,
				KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always
			});

		configureGrpcClient?.Invoke(grpcClientBuilder);

		serviceCollection.AddSingleton<IMessagePublisher, MessagePublisher>();

		serviceCollection.AddSingleton<IRtgsPublisher, RtgsPublisher>();
		serviceCollection.AddSingleton<IRtgsInternalPublisher, RtgsInternalPublisher>();

		serviceCollection.Configure<IdentityConfig>(identityConfig =>
		{
			identityConfig.ApiUrl = publisherOptions.IdCryptApiAddress.ToString();
			identityConfig.Apikey = publisherOptions.IdCryptApiKey;
			identityConfig.ServiceEndPoint = publisherOptions.IdCryptServiceEndPointAddress.ToString();
		});

		serviceCollection.AddHttpClient<IIdentityClient, IdentityClient>();
		serviceCollection.AddTransient<IIdentityClient, IdentityClient>();
		serviceCollection.AddSingleton<IRtgsConnectionBroker, RtgsConnectionBroker>();



		return serviceCollection;
	}
}
