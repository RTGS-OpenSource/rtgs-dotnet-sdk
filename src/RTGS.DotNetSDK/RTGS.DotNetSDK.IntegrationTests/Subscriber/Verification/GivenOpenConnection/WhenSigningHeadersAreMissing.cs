﻿namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.Verification.GivenOpenConnection;

public class WhenSigningHeadersAreMissing : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(500);
	private static readonly TimeSpan WaitForAcknowledgementsDuration = TimeSpan.FromMilliseconds(100);

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;
	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;

	public WhenSigningHeadersAreMissing(GrpcServerFixture grpcServer)
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
					TestData.ValidMessages.BankDid,
					_grpcServer.ServerUri,
					new Uri("http://id-crypt-cloud-agent-api.com"),
					"id-crypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
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
	public async Task WhenPrivateDidHeaderMissing_ThenLogError<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		var signingHeaders = new Dictionary<string, string>()
		{
			{ "alias", "alias" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

		await _rtgsSubscriber.StopAsync();

		_serilogContext.LogsFor($"RTGS.DotNetSDK.Subscriber.IdCrypt.Verification.{subscriberAction.MessageIdentifier}MessageVerifier", LogEventLevel.Error)
			.Should().ContainSingle().Which.Should().BeEquivalentTo(
				new LogEntry($"Private signature not found on {subscriberAction.MessageIdentifier} message, yet was expected", LogEventLevel.Error));
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task WhenAliasHeaderMissing_ThenLogError<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		var signingHeaders = new Dictionary<string, string>()
		{
			{ "pairwise-did-signature", "pairwise-did-signature" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

		await _rtgsSubscriber.StopAsync();

		_serilogContext.LogsFor($"RTGS.DotNetSDK.Subscriber.IdCrypt.Verification.{subscriberAction.MessageIdentifier}MessageVerifier", LogEventLevel.Error)
			.Should().ContainSingle().Which.Should().BeEquivalentTo(
				new LogEntry($"Alias not found on {subscriberAction.MessageIdentifier} message, yet was expected", LogEventLevel.Error));
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task WhenAliasHeaderMissing_ThenRaiseExceptionEvent<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		Exception raisedException = null;

		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);
		_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

		var signingHeaders = new Dictionary<string, string>()
		{
			{ "pairwise-did-signature", "pairwise-did-signature" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		raisedException.Should().BeOfType<RtgsSubscriberException>().Which.Message.Should().Be($"Unable to verify {subscriberAction.MessageIdentifier} message due to missing headers.");
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task WhenPrivateSignatureHeaderMissing_ThenRaiseExceptionEvent<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		Exception raisedException = null;

		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);
		_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

		var signingHeaders = new Dictionary<string, string>()
		{
			{ "alias", "alias" }
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		raisedException.Should().BeOfType<RtgsSubscriberException>().Which.Message.Should().Be($"Unable to verify {subscriberAction.MessageIdentifier} message due to missing headers.");
	}
}
