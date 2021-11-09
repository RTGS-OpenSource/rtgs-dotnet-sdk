namespace RTGSDotNetSDK.Publisher.Messages
{
	public record AtomicLockRequest
	{
		// TODO: properties
		public GenericFinancialIdentification1 DbtrToRtgsId { get; init; }
		public ActiveCurrencyAndAmount CdtrAmt { get; init; }
		public CashAccount38 UltmtDbtrAcct { get; init; }
		public CashAccount38 UltmtCdtrAcct { get; init; }
		public string SplmtryData { get; init; }
		public string EndToEndId { get; init; }
	}
}

namespace RTGSDotNetSDK.Publisher
{
	public interface IRtgsPublisher
	{
		
	}

	public record RtgsClientOptions
	{
		private RtgsClientOptions(Builder builder)
		{
			if (builder == null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			BankDid = builder.BankDidValue;
			RemoteHost = builder.RemoteHostValue;
		}

		public string BankDid { get; }
		public string RemoteHost { get; }

		public sealed class Builder
		{
			internal string BankDidValue { get; private set; }
			internal string RemoteHostValue { get; private set; }

			public static Builder CreateNew() => new();

			public Builder BankDid(string bankDid)
			{
				if (string.IsNullOrWhiteSpace(bankDid))
				{
					throw new ArgumentException("Value cannot be null or whitespace.", nameof(bankDid));
				}

				BankDidValue = bankDid;
				return this;
			}

			public Builder RemoteHost(string remoteHost)
			{
				if (string.IsNullOrWhiteSpace(remoteHost))
				{
					throw new ArgumentException("Value cannot be null or whitespace.", nameof(remoteHost));
				}

				RemoteHostValue = remoteHost;
				return this;
			}

			public Builder LoadConfig(IConfiguration configuration)
			{
				// TODO: check if not set
				var rtgsSection = configuration.GetSection("Rtgs");
				BankDid(rtgsSection["BankDid"]);
				RemoteHost(rtgsSection["RemoteHost"]);

				return this;
			}

			public RtgsClientOptions Build()
			{
				if (string.IsNullOrWhiteSpace(BankDidValue))
				{
					throw new InvalidOperationException($"The bank did has not been set");
				}

				if (string.IsNullOrWhiteSpace(RemoteHostValue))
				{
					throw new InvalidOperationException($"The remote host has not been set");
				}

				return new RtgsClientOptions(this);
			}
		}
	}
}

namespace RTGSDotNetSDK.Publisher.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRtgsClient(
			this IServiceCollection serviceCollection,
			RtgsClientOptions options,
			Action<IHttpClientBuilder> configureGrpcClient = null)
		{
			serviceCollection.AddSingleton(options);

			var grpcClientBuilder = serviceCollection.AddGrpcClient<Payment.PaymentClient>(client => client.Address = new Uri(options.RemoteHost))
				.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
				{
					PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
					// TODO: from config
					//KeepAlivePingDelay = TimeSpan.FromSeconds(config.GetValue("KeepAlivePingDelay", 30)),
					//KeepAlivePingTimeout = TimeSpan.FromSeconds(config.GetValue("KeepAlivePingTimeout", 30)),
					EnableMultipleHttp2Connections = true,
					KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always
				});

			configureGrpcClient?.Invoke(grpcClientBuilder);
			
			serviceCollection.AddTransient<IPublisher, Publisher>();

			return serviceCollection;
		}
	}
}
