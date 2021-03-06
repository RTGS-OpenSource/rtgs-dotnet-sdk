using System.Net.Http;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;
using RTGS.DotNetSDK.Subscriber.InternalMessages;
using ValidMessages = RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData.ValidMessages;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalHandlers.GivenPartnerBankEarmarkFundsV1SentToOpenSubscriberConnection;

public sealed class AndIdCryptVerifyMessageApiIsNotAvailable : IDisposable, IClassFixture<GrpcServerFixture>
{
	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;

	private StatusCodeHttpHandler _idCryptServiceHttpHandler;
	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;

	public AndIdCryptVerifyMessageApiIsNotAvailable(GrpcServerFixture grpcServer)
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
					new Uri("https://id-crypt-service"))
				.EnableMessageSigning()
				.Build();

			_idCryptServiceHttpHandler = StatusCodeHttpHandlerBuilderFactory
				.Create()
				.WithServiceUnavailableResponse(VerifyMessageUnsuccessfully.Path)
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
	public async Task WhenVerifyingMessage_ThenRaiseExceptionEvent()
	{
		using var exceptionSignal = new ManualResetEventSlim();

		Exception raisedException = null;

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, args) =>
		{
			raisedException = args.Exception;
			exceptionSignal.Set();
		};

		await _fromRtgsSender.SendAsync(nameof(PartnerBankEarmarkFundsV1), ValidMessages.PartnerBankEarmarkFundsV1, SubscriberActions.DefaultSigningHeaders);

		exceptionSignal.Wait();

		await _rtgsSubscriber.StopAsync();

		raisedException.Should().BeOfType<RtgsSubscriberException>()
			.Which.Message.Should().Be("Error occurred when sending VerifyMessage request to ID Crypt Service");
	}

	[Fact]
	public async Task WhenVerifyingMessage_ThenHandlerLogs()
	{
		using var exceptionSignal = new ManualResetEventSlim();

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, _) => exceptionSignal.Set();

		await _fromRtgsSender.SendAsync(nameof(PartnerBankEarmarkFundsV1), ValidMessages.PartnerBankEarmarkFundsV1, SubscriberActions.DefaultSigningHeaders);

		exceptionSignal.Wait();

		await _rtgsSubscriber.StopAsync();

		var errorLogs = _serilogContext.LogsForNamespace("RTGS.DotNetSDK.Subscriber.IdCrypt.Verification", LogEventLevel.Error);
		errorLogs.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
			"Error occurred when sending VerifyMessage request to ID Crypt Service",
			LogEventLevel.Error,
			typeof(RtgsSubscriberException)));
	}

	[Fact]
	public async Task WhenVerifyingMessage_ThenIdCryptServiceClientLogs()
	{
		using var exceptionSignal = new ManualResetEventSlim();

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, _) => exceptionSignal.Set();

		await _fromRtgsSender.SendAsync(nameof(PartnerBankEarmarkFundsV1), ValidMessages.PartnerBankEarmarkFundsV1, SubscriberActions.DefaultSigningHeaders);

		exceptionSignal.Wait();

		await _rtgsSubscriber.StopAsync();

		using var _ = new AssertionScope();

		var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.IdCrypt.IdCryptServiceClient", LogEventLevel.Debug);
		debugLogs.Should().ContainSingle()
			.Which.Should().BeEquivalentTo(new LogEntry("Sending VerifyMessage request to ID Crypt Service", LogEventLevel.Debug));

		var errorLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.IdCrypt.IdCryptServiceClient", LogEventLevel.Error);
		errorLogs.Should().ContainSingle()
			.Which.Should().BeEquivalentTo(new LogEntry(
				"Error occurred when sending VerifyMessage request to ID Crypt Service",
				LogEventLevel.Error,
				typeof(HttpRequestException)));
	}
}
