namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.Verification.GivenOpenConnection;

public sealed class AndSigningHeadersAreMissing : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan WaitForAcknowledgementsDuration = TimeSpan.FromMilliseconds(100);

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;

	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;

	public AndSigningHeadersAreMissing(GrpcServerFixture grpcServer)
	{
		_grpcServer = grpcServer;

		SetupSerilogLogger();

		SetupDependencies();

		_serilogContext = TestCorrelator.CreateContext();
	}

	private static void SetupSerilogLogger() =>
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Debug()
			.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
			.Enrich.FromLogContext()
			.WriteTo.Console()
			.WriteTo.TestCorrelator()
			.CreateLogger();

	private void SetupDependencies()
	{
		try
		{
			var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
					TestData.ValidMessages.RtgsGlobalId,
					_grpcServer.ServerUri,
					new Uri("https://id-crypt-service"))
				.EnableMessageSigning()
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices((_, services) => services
					.AddRtgsSubscriber(rtgsSdkOptions))
				.UseSerilog()
				.Build();

			_fromRtgsSender = _grpcServer.Services.GetRequiredService<FromRtgsSender>();
			_rtgsSubscriber = _clientHost.Services.GetRequiredService<IRtgsSubscriber>();
		}
		catch (Exception)
		{
			Dispose();

			throw;
		}
	}

	public void Dispose()
	{
		_clientHost?.Dispose();

		_grpcServer.Reset();
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndPairwiseDidHeaderMissing_WhenVerifyingMessage_ThenLogError<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		// Intermediary solution until we have decided how to handle unsigned MessageRejectV1 messages from RTGS.Global
		if (subscriberAction.MessageIdentifier == nameof(MessageRejectV1))
		{
			return;
		}

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		var signingHeaders = new Dictionary<string, string>
		{
			{ "public-did-signature", "public-did-signature" },
			{ "alias", "alias" },
			{ "from-rtgs-global-id", "from-rtgs-global-id" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		_serilogContext
			.LogsFor($"RTGS.DotNetSDK.Subscriber.Adapters.DataVerifyingMessageAdapter", LogEventLevel.Error)
			.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
				$"Private signature not found on {subscriberAction.MessageIdentifier} message, yet was expected",
				LogEventLevel.Error));
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndAliasHeaderMissing_WhenVerifyingMessage_ThenLogError<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		var signingHeaders = new Dictionary<string, string>
		{
			{ "public-did-signature", "public-did-signature" },
			{ "pairwise-did-signature", "pairwise-did-signature" },
			{ "from-rtgs-global-id", "from-rtgs-global-id" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		_serilogContext
			.LogsFor(
				$"RTGS.DotNetSDK.Subscriber.Adapters.DataVerifyingMessageAdapter",
				LogEventLevel.Error)
			.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
				$"Alias not found on {subscriberAction.MessageIdentifier} message, yet was expected",
				LogEventLevel.Error));
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndFromRtgsGlobalIdHeaderMissing_WhenVerifyingMessage_ThenLogError<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		var signingHeaders = new Dictionary<string, string>
		{
			{ "pairwise-did-signature", "pairwise-did-signature" },
			{ "public-did-signature", "public-did-signature" },
			{ "alias", "alias" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		_serilogContext
			.LogsFor(
				$"RTGS.DotNetSDK.Subscriber.Adapters.DataVerifyingMessageAdapter",
				LogEventLevel.Error)
			.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
				$"From RTGS Global ID not found on {subscriberAction.MessageIdentifier} message, yet was expected",
				LogEventLevel.Error));
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndPublicDidSignatureHeaderMissing_WhenVerifyingMessage_ThenLogError<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		var signingHeaders = new Dictionary<string, string>
		{
			{ "pairwise-did-signature", "pairwise-did-signature" },
			{ "alias", "alias" },
			{ "from-rtgs-global-id", "from-rtgs-global-id" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		_serilogContext
			.LogsFor(
				$"RTGS.DotNetSDK.Subscriber.Adapters.DataVerifyingMessageAdapter",
				LogEventLevel.Error)
			.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
				$"Public signature not found on {subscriberAction.MessageIdentifier} message, yet was expected",
				LogEventLevel.Error));
	}
	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndAliasHeaderMissing_WhenVerifyingMessage_ThenRaiseExceptionEvent<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		Exception raisedException = null;

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

		var signingHeaders = new Dictionary<string, string>
		{
			{ "pairwise-did-signature", "pairwise-did-signature" },
			{ "public-did-signature", "public-did-signature" },
			{ "from-rtgs-global-id", "from-rtgs-global-id" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		raisedException.Should().BeOfType<VerificationFailedException>().Which.Message.Should()
			.Be($"Unable to verify {subscriberAction.MessageIdentifier} message due to missing headers.");
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndPairwiseSignatureHeaderMissing_WhenVerifyingMessage_ThenRaiseExceptionEvent<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		// Intermediary solution until we have decided how to handle unsigned MessageRejectV1 messages from RTGS.Global
		if (subscriberAction.MessageIdentifier == nameof(MessageRejectV1))
		{
			return;
		}

		Exception raisedException = null;

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

		var signingHeaders = new Dictionary<string, string>
		{
			{ "public-did-signature", "public-did-signature" },
			{ "alias", "alias" },
			{ "from-rtgs-global-id", "from-rtgs-global-id" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		raisedException.Should().BeOfType<VerificationFailedException>().Which.Message.Should()
			.Be($"Unable to verify {subscriberAction.MessageIdentifier} message due to missing headers.");
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndFromRtgsGlobalIdHeaderMissing_WhenVerifyingMessage_ThenRaiseExceptionEvent<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		Exception raisedException = null;

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

		var signingHeaders = new Dictionary<string, string>
		{
			{ "pairwise-did-signature", "pairwise-did-signature" },
			{ "public-did-signature", "public-did-signature" },
			{ "alias", "alias" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		raisedException.Should().BeOfType<VerificationFailedException>().Which.Message.Should()
			.Be($"Unable to verify {subscriberAction.MessageIdentifier} message due to missing headers.");
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndPublicDidSignatureHeaderMissing_WhenVerifyingMessage_ThenRaiseExceptionEvent<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		Exception raisedException = null;

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

		var signingHeaders = new Dictionary<string, string>
		{
			{ "pairwise-did-signature", "pairwise-did-signature" },
			{ "alias", "alias" },
			{ "from-rtgs-global-id", "from-rtgs-global-id" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		raisedException.Should().BeOfType<VerificationFailedException>().Which.Message.Should()
			.Be($"Unable to verify {subscriberAction.MessageIdentifier} message due to missing headers.");
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndPairwiseDidHeaderEmpty_WhenVerifyingMessage_ThenLogError<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		var signingHeaders = new Dictionary<string, string>
		{
			{ "pairwise-did-signature", string.Empty },
			{ "public-did-signature", "public-did-signature" },
			{ "alias", "alias" },
			{ "from-rtgs-global-id", "from-rtgs-global-id" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		_serilogContext
			.LogsFor($"RTGS.DotNetSDK.Subscriber.Adapters.DataVerifyingMessageAdapter", LogEventLevel.Error)
			.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
				$"Private signature not found on {subscriberAction.MessageIdentifier} message, yet was expected",
				LogEventLevel.Error));
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndAliasHeaderEmpty_WhenVerifyingMessage_ThenLogError<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		var signingHeaders = new Dictionary<string, string>
		{
			{ "pairwise-did-signature", "pairwise-did-signature" },
			{ "public-did-signature", "public-did-signature" },
			{ "alias", string.Empty },
			{ "from-rtgs-global-id", "from-rtgs-global-id" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		_serilogContext.LogsFor($"RTGS.DotNetSDK.Subscriber.Adapters.DataVerifyingMessageAdapter", LogEventLevel.Error)
			.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
				$"Alias not found on {subscriberAction.MessageIdentifier} message, yet was expected",
				LogEventLevel.Error));
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndFromRtgsGlobalIdHeaderEmpty_WhenVerifyingMessage_ThenLogError<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		var signingHeaders = new Dictionary<string, string>
		{
			{ "pairwise-did-signature", "pairwise-did-signature" },
			{ "public-did-signature", "public-did-signature" },
			{ "alias", "alias" },
			{ "from-rtgs-global-id", string.Empty }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		_serilogContext.LogsFor($"RTGS.DotNetSDK.Subscriber.Adapters.DataVerifyingMessageAdapter", LogEventLevel.Error)
			.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
				$"From RTGS Global ID not found on {subscriberAction.MessageIdentifier} message, yet was expected",
				LogEventLevel.Error));
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndPublicDidSignatureHeaderEmpty_WhenVerifyingMessage_ThenLogError<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		var signingHeaders = new Dictionary<string, string>
		{
			{ "pairwise-did-signature", "pairwise-did-signature" },
			{ "public-did-signature", string.Empty},
			{ "alias", "alias" },
			{ "from-rtgs-global-id", "from-rtgs-global-id" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		_serilogContext.LogsFor($"RTGS.DotNetSDK.Subscriber.Adapters.DataVerifyingMessageAdapter", LogEventLevel.Error)
			.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
				$"Public signature not found on {subscriberAction.MessageIdentifier} message, yet was expected",
				LogEventLevel.Error));
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndPairwiseSignatureHeaderEmpty_WhenVerifyingMessage_ThenRaiseExceptionEvent<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		Exception raisedException = null;

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

		var signingHeaders = new Dictionary<string, string>
		{
			{ "pairwise-did-signature", string.Empty },
			{ "public-did-signature", "public-did-signature"},
			{ "alias", "alias" },
			{ "from-rtgs-global-id", "from-rtgs-global-id" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		raisedException.Should().BeOfType<VerificationFailedException>().Which.Message.Should()
			.Be($"Unable to verify {subscriberAction.MessageIdentifier} message due to missing headers.");
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndAliasHeaderEmpty_WhenVerifyingMessage_ThenRaiseExceptionEvent<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		Exception raisedException = null;

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

		var signingHeaders = new Dictionary<string, string>
		{
			{ "pairwise-did-signature", "pairwise-did-signature" },
			{ "public-did-signature", "public-did-signature"},
			{ "alias", string.Empty },
			{ "from-rtgs-global-id", "from-rtgs-global-id" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		raisedException.Should().BeOfType<VerificationFailedException>().Which.Message.Should()
			.Be($"Unable to verify {subscriberAction.MessageIdentifier} message due to missing headers.");
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndFromRtgsGlobalIdHeaderHeaderEmpty_WhenVerifyingMessage_ThenRaiseExceptionEvent<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		Exception raisedException = null;

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

		var signingHeaders = new Dictionary<string, string>
		{
			{ "pairwise-did-signature", "pairwise-did-signature" },
			{ "public-did-signature", "public-did-signature"},
			{ "alias", "alias" },
			{ "from-rtgs-global-id" , string.Empty }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		raisedException.Should().BeOfType<VerificationFailedException>().Which.Message.Should()
			.Be($"Unable to verify {subscriberAction.MessageIdentifier} message due to missing headers.");
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndPublicDidSignatureHeaderHeaderEmpty_WhenVerifyingMessage_ThenRaiseExceptionEvent<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		Exception raisedException = null;

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

		var signingHeaders = new Dictionary<string, string>
		{
			{ "pairwise-did-signature", "pairwise-did-signature" },
			{ "public-did-signature", string.Empty},
			{ "alias", "alias" },
			{ "from-rtgs-global-id" , "from-rtgs-global-id" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		raisedException.Should().BeOfType<VerificationFailedException>().Which.Message.Should()
			.Be($"Unable to verify {subscriberAction.MessageIdentifier} message due to missing headers.");
	}
}
