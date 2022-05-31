using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using RTGS.DotNetSDK.IdCrypt;
using RTGS.DotNetSDK.Publisher;
using RTGS.DotNetSDK.Publisher.IdCrypt;
using RTGS.DotNetSDK.Publisher.IdCrypt.Signing;
using RTGS.DotNetSDK.Subscriber;
using RTGS.DotNetSDK.Subscriber.Adapters;
using RTGS.DotNetSDK.Subscriber.HandleMessageCommands;
using RTGS.DotNetSDK.Subscriber.Handlers.Internal;
using RTGS.DotNetSDK.Subscriber.IdCrypt.Verification;
using RTGS.DotNetSDK.Subscriber.InternalMessages;
using RTGS.DotNetSDK.Subscriber.Validators;
using RTGS.Public.Messages.Publisher;
using RTGS.Public.Messages.Subscriber;
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

		serviceCollection.AddSingleton<ISignMessage<AtomicLockRequestV1>, AtomicLockRequestV1MessageSigner>();

		serviceCollection.AddSingleton<ISignMessage<PayawayCreationV1>, PayawayCreationV1MessageSigner>();
		serviceCollection.AddSingleton<ISignMessage<PayawayRejectionV1>, PayawayRejectionV1MessageSigner>();
		serviceCollection.AddSingleton<ISignMessage<PayawayConfirmationV1>, PayawayConfirmationV1MessageSigner>();

		serviceCollection
			.AddHttpClient("IdCryptServiceClient", client =>
			{
				client.BaseAddress = options.IdCryptServiceAddress;
			})
			.AddTypedClient<IIdCryptServiceClient, IdCryptServiceClient>();

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
		serviceCollection.AddTransient<IMessageAdapter, DataOnlyMessageAdapter<AtomicLockResponseV1>>();
		serviceCollection.AddTransient<IMessageAdapter, DataOnlyMessageAdapter<AtomicTransferResponseV1>>();
		serviceCollection.AddTransient<IMessageAdapter, DataOnlyMessageAdapter<AtomicTransferFundsV1>>();

		serviceCollection.AddTransient<IMessageAdapter, DataOnlyMessageAdapter<EarmarkFundsV1>>();
		serviceCollection.AddTransient<IMessageAdapter, DataOnlyMessageAdapter<InitiatingBankEarmarkFundsV1>>();
		serviceCollection.AddTransient<IMessageAdapter, DataOnlyMessageAdapter<PartnerBankEarmarkFundsV1>>();

		serviceCollection.AddTransient<IMessageAdapter, DataOnlyMessageAdapter<EarmarkCompleteV1>>();
		serviceCollection.AddTransient<IMessageAdapter, DataOnlyMessageAdapter<EarmarkReleaseV1>>();
		serviceCollection.AddTransient<IMessageAdapter, DataOnlyMessageAdapter<BankPartnersResponseV1>>();
		serviceCollection.AddTransient<IMessageAdapter, IdCryptCreateInvitationRequestV1MessageAdapter>();
		serviceCollection.AddTransient<IMessageAdapter, IdCryptBankInvitationV1MessageAdapter>();

		serviceCollection.AddTransient<IMessageAdapter, DataVerifyingMessageAdapter<PayawayFundsV1>>();
		serviceCollection.AddTransient<IMessageAdapter, DataVerifyingMessageAdapter<MessageRejectV1>>();
		serviceCollection.AddTransient<IMessageAdapter, DataVerifyingMessageAdapter<PayawayCompleteV1>>();

		serviceCollection.AddSingleton<IHandlerValidator, HandlerValidator>();

		serviceCollection.AddTransient<IInternalHandler, IdCryptCreateInvitationRequestV1Handler>();
		serviceCollection.AddTransient<IInternalHandler, IdCryptBankInvitationV1Handler>();
		serviceCollection.AddTransient<IInternalHandler, InitiatingBankEarmarkFundsV1Handler>();
		serviceCollection.AddTransient<IInternalHandler, PartnerBankEarmarkFundsV1Handler>();

		serviceCollection.AddSingleton<IVerifyMessage<PayawayFundsV1>, PayawayFundsV1MessageVerifier>();
		serviceCollection.AddSingleton<IVerifyMessage<MessageRejectV1>, MessageRejectV1MessageVerifier>();
		serviceCollection.AddSingleton<IVerifyMessage<PayawayCompleteV1>, PayawayCompleteV1MessageVerifier>();

		serviceCollection.AddSingleton<IIdCryptPublisher, IdCryptPublisher>();
		serviceCollection.AddSingleton<IInternalPublisher, InternalPublisher>();

		serviceCollection
			.AddHttpClient("IdCryptServiceClient", client =>
			{
				client.BaseAddress = options.IdCryptServiceAddress;
			})
			.AddTypedClient<IIdCryptServiceClient, IdCryptServiceClient>();

		return serviceCollection;
	}

}
