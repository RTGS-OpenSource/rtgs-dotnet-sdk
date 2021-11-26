using System;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Publisher.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRtgsPublisher(
			this IServiceCollection serviceCollection,
			RtgsClientOptions options,
			Action<IHttpClientBuilder> configureGrpcClient = null)
		{
			serviceCollection.AddSingleton(options);

			var grpcClientBuilder = serviceCollection.AddGrpcClient<Payment.PaymentClient>(
				clientOptions => clientOptions.Address = options.RemoteHostAddress).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
				{
					PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
					KeepAlivePingDelay = options.KeepAlivePingDelay,
					KeepAlivePingTimeout = options.KeepAlivePingTimeout,
					EnableMultipleHttp2Connections = true,
					KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always
				});

			configureGrpcClient?.Invoke(grpcClientBuilder);

			serviceCollection.AddTransient<IRtgsPublisher, RtgsPublisher>();

			return serviceCollection;
		}
	}
}
