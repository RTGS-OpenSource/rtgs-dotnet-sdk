using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.InternalMessages;
using ValidMessages = RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData.ValidMessages;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalHandlers.GivenInitiatingBankEarmarkFundsV1SentToOpenSubscriberConnection;

public sealed class AndSignaturesAreValid : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(1_000);
	private static readonly TimeSpan WaitForSubscriberAcknowledgementDuration = TimeSpan.FromMilliseconds(100);

	private readonly List<IHandler> _allTestHandlers = new AllTestHandlers().ToList();

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;

	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;
	private ToRtgsMessageHandler _toRtgsMessageHandler;
	private StatusCodeHttpHandler _idCryptServiceHttpHandler
		;

	public AndSignaturesAreValid(GrpcServerFixture grpcServer)
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
					ValidMessages.RtgsGlobalId,
					_grpcServer.ServerUri,
					new("https://id-crypt-service"))
				.EnableMessageSigning()
				.Build();

			_idCryptServiceHttpHandler = StatusCodeHttpHandlerBuilderFactory
				.Create()
				.WithOkResponse(VerifyOwnMessageSuccessfully.HttpRequestResponseContext)
				.Build();


			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices((_, services) => services
					.AddRtgsSubscriber(rtgsSdkOptions)
					.AddTestIdCryptServiceHttpClient(_idCryptServiceHttpHandler))
				.UseSerilog()
				.Build();

			_fromRtgsSender = _grpcServer.Services.GetRequiredService<FromRtgsSender>();
			_rtgsSubscriber = _clientHost.Services.GetRequiredService<IRtgsSubscriber>();
			_toRtgsMessageHandler = _grpcServer.Services.GetRequiredService<ToRtgsMessageHandler>();
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

	[Fact]
	public async Task WhenMessageReceived_ThenSeeRtgsGlobalIdInRequestHeader()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync(nameof(InitiatingBankEarmarkFundsV1), ValidMessages.InitiatingBankEarmarkFundsV1, SubscriberActions.DefaultSigningHeaders);

		_fromRtgsSender.RequestHeaders.Should().ContainSingle(header =>
			header.Key == "rtgs-global-id"
			&& header.Value == ValidMessages.RtgsGlobalId);
	}

	[Fact]
	public async Task WhenMessageReceived_ThenAcknowledge()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		var sentRtgsMessage = await _fromRtgsSender.SendAsync(nameof(InitiatingBankEarmarkFundsV1), ValidMessages.InitiatingBankEarmarkFundsV1, SubscriberActions.DefaultSigningHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		_fromRtgsSender.Acknowledgements.Should().ContainSingle(acknowledgement =>
			acknowledgement.CorrelationId == sentRtgsMessage.CorrelationId && acknowledgement.Success);
	}

	[Fact]
	public async Task WhenMessageReceived_ThenHandledByEarmarkFundsV1Handler()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync(nameof(InitiatingBankEarmarkFundsV1), ValidMessages.InitiatingBankEarmarkFundsV1, SubscriberActions.DefaultSigningHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		var handler = _allTestHandlers.GetHandler<EarmarkFundsV1>();
		handler.WaitForMessage(WaitForReceivedMessageDuration);

		handler.ReceivedMessage.Should().BeEquivalentTo(new EarmarkFundsV1
		{
			LckId = ValidMessages.InitiatingBankEarmarkFundsV1.LckId,
			Amt = ValidMessages.InitiatingBankEarmarkFundsV1.DbtrAmt,
			Acct = ValidMessages.InitiatingBankEarmarkFundsV1.DbtrAcct
		});

		await _rtgsSubscriber.StopAsync();
	}

	[Fact]
	public async Task WhenMessageReceived_ThenLogInformation()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync(nameof(InitiatingBankEarmarkFundsV1), ValidMessages.InitiatingBankEarmarkFundsV1, SubscriberActions.DefaultSigningHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		var handler = _allTestHandlers.OfType<AllTestHandlers.TestHandler<EarmarkFundsV1>>().Single();
		handler.WaitForMessage(WaitForReceivedMessageDuration);

		await _rtgsSubscriber.StopAsync();

		var informationLogs = _serilogContext.LogsForNamespace("RTGS.DotNetSDK.Subscriber", LogEventLevel.Information);
		var expectedLogs = new List<LogEntry>
		{
			new("RTGS Subscriber started", LogEventLevel.Information),
			new("InitiatingBankEarmarkFundsV1 message received from RTGS", LogEventLevel.Information),
			new("Verifying InitiatingBankEarmarkFundsV1 message", LogEventLevel.Information),
			new("Verified InitiatingBankEarmarkFundsV1 message", LogEventLevel.Information),
			new("RTGS Subscriber stopping", LogEventLevel.Information),
			new("RTGS Subscriber stopped", LogEventLevel.Information)
		};

		informationLogs.Should().BeEquivalentTo(expectedLogs, options => options.WithStrictOrdering());
	}
}
