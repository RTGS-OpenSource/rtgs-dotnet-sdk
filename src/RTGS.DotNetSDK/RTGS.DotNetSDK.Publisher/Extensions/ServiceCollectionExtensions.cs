using Microsoft.Extensions.DependencyInjection;
using RTGS.Public.Payment.V2;
using System;

namespace RTGS.DotNetSDK.Publisher.Extensions
{
	/// <summary>
	/// ServiceCollection Extensions class
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds <seealso cref="IRtgsPublisher"/> with supplied client configuration of <seealso cref="RtgsClientOptions"/>.
		/// </summary>
		/// <param name="serviceCollection">The service collection</param>
		/// <param name="options">The client options</param>
		/// <param name="configureGrpcClient">The client configure action (optional)</param>
		/// <returns>The service collection so that additional calls can be chained.</returns>
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
