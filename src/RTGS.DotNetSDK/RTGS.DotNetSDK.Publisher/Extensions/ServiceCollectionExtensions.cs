using System;
using Microsoft.Extensions.DependencyInjection;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Publisher.Extensions
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds a publisher to the service collection.
		/// </summary>
		/// <param name="serviceCollection">The service collection</param>
		/// <param name="options">Client options</param>
		/// <param name="configureGrpcClient">Grpc client configuration</param>
		/// <returns>IServiceCollection - The IServiceCollection so that additional calls can be chained.</returns>
		public static IServiceCollection AddRtgsPublisher(
			this IServiceCollection serviceCollection,
			RtgsClientOptions options,
			Action<IHttpClientBuilder> configureGrpcClient = null)
		{
			serviceCollection.AddSingleton(options);

			// TODO: include ConfigurePrimaryHttpMessageHandler for keep alive?
			var grpcClientBuilder = serviceCollection.AddGrpcClient<Payment.PaymentClient>(
				clientOptions => clientOptions.Address = options.RemoteHostAddress);

			configureGrpcClient?.Invoke(grpcClientBuilder);

			serviceCollection.AddTransient<IRtgsPublisher, RtgsPublisher>();

			return serviceCollection;
		}
	}
}
