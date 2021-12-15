using Microsoft.Extensions.DependencyInjection;
using RTGS.DotNetSDK.Subscriber.Adapters;
using RTGS.DotNetSDK.Subscriber.HandleMessageCommands;
using RTGS.DotNetSDK.Subscriber.Validators;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Extensions
{
	/// <summary>
	/// Extensions for implementations of <see cref="IServiceCollection"/>.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds <seealso cref="IRtgsSubscriber"/> with supplied client configuration of <seealso cref="RtgsSubscriberOptions"/>.
		/// </summary>
		/// <param name="serviceCollection">The service collection</param>
		/// <param name="options">The options used to build the gRPC client</param>
		/// <param name="configureGrpcClient">The client configure action (optional)</param>
		/// <returns>The service collection so that additional calls can be chained.</returns>
		public static IServiceCollection AddRtgsSubscriber(
			this IServiceCollection serviceCollection,
			RtgsSubscriberOptions options,
			Action<IHttpClientBuilder> configureGrpcClient = null)
		{
			serviceCollection.AddSingleton(options);

			var grpcClientBuilder = serviceCollection.AddGrpcClient<Payment.PaymentClient>(
				clientOptions => clientOptions.Address = options.RemoteHostAddress);

			configureGrpcClient?.Invoke(grpcClientBuilder);

			serviceCollection.AddTransient<IRtgsSubscriber, RtgsSubscriber>();
			serviceCollection.AddTransient<IHandleMessageCommandsFactory, HandleMessageCommandsFactory>();
			serviceCollection.AddTransient<IMessageAdapter, AtomicLockResponseV1MessageAdapter>();
			serviceCollection.AddTransient<IMessageAdapter, AtomicTransferResponseV1MessageAdapter>();
			serviceCollection.AddTransient<IMessageAdapter, AtomicTransferFundsV1MessageAdapter>();
			serviceCollection.AddTransient<IMessageAdapter, MessageRejectedV1MessageAdapter>();
			serviceCollection.AddTransient<IMessageAdapter, PayawayCompleteV1MessageAdapter>();
			serviceCollection.AddTransient<IMessageAdapter, PayawayFundsV1MessageAdapter>();
			serviceCollection.AddTransient<IMessageAdapter, EarmarkFundsV1MessageAdapter>();
			serviceCollection.AddTransient<IMessageAdapter, EarmarkCompleteV1MessageAdapter>();
			serviceCollection.AddTransient<IMessageAdapter, EarmarkReleaseV1MessageAdapter>();
			serviceCollection.AddSingleton<IHandlerValidator, HandlerValidator>();

			return serviceCollection;
		}
	}
}
