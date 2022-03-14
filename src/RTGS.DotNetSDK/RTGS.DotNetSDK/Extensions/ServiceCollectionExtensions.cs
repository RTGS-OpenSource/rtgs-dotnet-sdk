using System.Net.Http;
using IDCryptGlobal.Cloud.Agent.Identity;
using Microsoft.Extensions.DependencyInjection;
using RTGS.DotNetSDK.IdCrypt;
using RTGS.DotNetSDK.Publisher;
using RTGS.DotNetSDK.Subscriber;
using RTGS.DotNetSDK.Subscriber.Adapters;
using RTGS.DotNetSDK.Subscriber.HandleMessageCommands;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Handlers.Internal;
using RTGS.DotNetSDK.Subscriber.Validators;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Extensions;

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
		RtgsSdkOptions publisherOptions,
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

		serviceCollection.AddSingleton<IInternalPublisher, InternalPublisher>();

		serviceCollection.AddSingleton<IRtgsPublisher, RtgsPublisher>();
		serviceCollection.AddSingleton<IIdCryptPublisher, IdCryptPublisher>();

		serviceCollection.Configure<IdentityConfig>(identityConfig =>
		{
			identityConfig.ApiUrl = publisherOptions.IdCryptApiAddress.ToString();
			identityConfig.Apikey = publisherOptions.IdCryptApiKey;
			identityConfig.ServiceEndPoint = publisherOptions.IdCryptServiceEndPointAddress.ToString();
		});

		serviceCollection.AddHttpClient<IIdentityClient, IdentityClient>();
		serviceCollection.AddTransient<IIdentityClient, IdentityClient>();
		serviceCollection.AddTransient<IRtgsConnectionBroker, RtgsConnectionBroker>();

		return serviceCollection;
	}

	/// <summary>
	/// Adds <seealso cref="IRtgsSubscriber"/> with supplied client configuration of <seealso cref="RtgsSubscriberOptions"/>.
	/// </summary>
	/// <param name="serviceCollection">The service collection</param>
	/// <param name="options">The options used to build the gRPC client</param>
	/// <param name="configureGrpcClient">The client configure action (optional)</param>
	/// <returns>The service collection so that additional calls can be chained.</returns>
	public static IServiceCollection AddRtgsSubscriber(
		this IServiceCollection serviceCollection,
		RtgsSdkOptions options,
		Action<IHttpClientBuilder> configureGrpcClient = null)
	{
		serviceCollection.AddSingleton(options);

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

		serviceCollection.AddSingleton<IRtgsSubscriber, RtgsSubscriber>();
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
		serviceCollection.AddTransient<IMessageAdapter, BankPartnersResponseV1MessageAdapter>();
		serviceCollection.AddTransient<IMessageAdapter, IdCryptInvitationConfirmationV1MessageAdapter>();
		serviceCollection.AddTransient<IMessageAdapter, IdCryptCreateInvitationRequestV1MessageAdapter>();
		serviceCollection.AddSingleton<IHandlerValidator, HandlerValidator>();

		serviceCollection.AddTransient<IDependentHandler, IdCryptCreateInvitationRequestV1Handler>();

		return serviceCollection;
	}

}
