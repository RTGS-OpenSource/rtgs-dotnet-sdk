using System.Net.Http;
using IDCryptGlobal.Cloud.Agent.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
	/// <param name="options">The options used to build the gRPC client</param>
	/// <param name="configureGrpcClient">The client configure action (optional)</param>
	/// <returns>The service collection so that additional calls can be chained.</returns>
	public static IServiceCollection AddRtgsPublisher(
		this IServiceCollection serviceCollection,
		RtgsPublisherOptions options,
		Action<IHttpClientBuilder> configureGrpcClient = null)
	{
		serviceCollection.AddSingleton(options);
		serviceCollection.AddSingleton(Options.Create(options.IdentityConfig));

		var grpcClientBuilder = serviceCollection
			.AddGrpcClient<Payment.PaymentClient>(clientOptions => clientOptions.Address = options.RemoteHostAddress)
			.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
			{
				PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
				KeepAlivePingDelay = options.KeepAlivePingDelay,
				KeepAlivePingTimeout = options.KeepAlivePingTimeout,
				EnableMultipleHttp2Connections = true,
				KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always
			});

		configureGrpcClient?.Invoke(grpcClientBuilder);

		serviceCollection.AddHttpClient<IIdentityClient, IdentityClient>();
		serviceCollection.AddTransient<IIdentityClient, IdentityClient>();
		serviceCollection.AddSingleton<IRtgsInternalPublisher, RtgsInternalPublisher>();

		serviceCollection.AddSingleton<IRtgsConnectionBroker, RtgsConnectionBroker>();

		serviceCollection.AddSingleton<IRtgsPublisher, RtgsPublisher>();

		return serviceCollection;
	}
}
