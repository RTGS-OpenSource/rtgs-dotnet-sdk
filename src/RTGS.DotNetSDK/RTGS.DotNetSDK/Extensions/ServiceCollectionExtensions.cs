using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using RTGS.DotNetSDK.Publisher;
using RTGS.DotNetSDK.Publisher.IdCrypt;
using RTGS.DotNetSDK.Publisher.IdCrypt.Signing;
using RTGS.DotNetSDK.Subscriber;
using RTGS.DotNetSDK.Subscriber.Adapters;
using RTGS.DotNetSDK.Subscriber.HandleMessageCommands;
using RTGS.DotNetSDK.Subscriber.Handlers.Internal;
using RTGS.DotNetSDK.Subscriber.IdCrypt.Verification;
using RTGS.DotNetSDK.Subscriber.Validators;
using RTGS.IDCryptSDK;
using RTGS.IDCryptSDK.Extensions;
using RTGS.Public.Messages.Publisher;
using RTGS.Public.Payment.V4;

namespace RTGS.DotNetSDK.Extensions;

/// <summary>
/// ServiceCollection Extensions class
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds <seealso cref="IRtgsPublisher"/> with supplied client configuration of <seealso cref="RtgsSdkOptions"/>.
	/// </summary>
	/// <param name="serviceCollection">The service collection</param>
	/// <param name="options">The options used to build the publisher</param>
	/// <param name="configureGrpcClient">The client configure action (optional)</param>
	/// <returns>The service collection so that additional calls can be chained.</returns>
	public static IServiceCollection AddRtgsPublisher(
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

		serviceCollection.AddSingleton<IInternalPublisher, InternalPublisher>();

		serviceCollection.AddSingleton<IRtgsPublisher, RtgsPublisher>();

		serviceCollection.AddSingleton<IIdCryptPublisher, IdCryptPublisher>();

		serviceCollection.AddTransient<IRtgsConnectionBroker, RtgsConnectionBroker>();

		serviceCollection.AddSingleton<ISignMessage<PayawayCreationV1>, PayawayCreateMessageSigner>();
		serviceCollection.AddSingleton<ISignMessage<PayawayRejectionV1>, PayawayRejectMessageSigner>();
		serviceCollection.AddSingleton<ISignMessage<PayawayConfirmationV1>, PayawayConfirmMessageSigner>();

		serviceCollection.AddIdCryptSdk(new IdCryptSdkConfiguration(
			options.IdCryptApiAddress,
			options.IdCryptApiKey,
			options.IdCryptServiceEndpointAddress));

		return serviceCollection;
	}

	/// <summary>
	/// Adds <seealso cref="IRtgsSubscriber"/> with supplied client configuration of <seealso cref="RtgsSdkOptions"/>.
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
		serviceCollection.AddTransient<IMessageAdapter, IdCryptBankInvitationV1MessageAdapter>();
		serviceCollection.AddSingleton<IHandlerValidator, HandlerValidator>();

		serviceCollection.AddTransient<IInternalHandler, IdCryptCreateInvitationRequestV1Handler>();
		serviceCollection.AddTransient<IInternalHandler, IdCryptBankInvitationV1Handler>();

		serviceCollection.AddSingleton<IVerifyMessage, PayawayFundsV1MessageVerifier>();

		serviceCollection.AddSingleton<IIdCryptPublisher, IdCryptPublisher>();
		serviceCollection.AddSingleton<IInternalPublisher, InternalPublisher>();

		serviceCollection.AddIdCryptSdk(new IdCryptSdkConfiguration(
			options.IdCryptApiAddress,
			options.IdCryptApiKey,
			options.IdCryptServiceEndpointAddress));

		return serviceCollection;
	}

}
