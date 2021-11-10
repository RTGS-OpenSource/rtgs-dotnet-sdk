using System;
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

			// TODO: Might need to pass in Action<GrpcClientFactoryOptions> configureClient instead of RtgsClientOptions - Think it will always try to use RemoteHostAddress
			// TODO: include ConfigurePrimaryHttpMessageHandler for keep alive?
			var grpcClientBuilder = serviceCollection.AddGrpcClient<Payment.PaymentClient>(
				clientOptions => clientOptions.Address = options.RemoteHostAddress);

			configureGrpcClient?.Invoke(grpcClientBuilder);

			serviceCollection.AddTransient<IRtgsPublisher, RtgsPublisher>();

			return serviceCollection;
		}
	}
}
