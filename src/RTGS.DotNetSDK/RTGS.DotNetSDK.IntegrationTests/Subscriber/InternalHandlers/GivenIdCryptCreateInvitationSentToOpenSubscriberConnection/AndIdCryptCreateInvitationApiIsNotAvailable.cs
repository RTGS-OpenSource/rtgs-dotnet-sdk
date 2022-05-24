using System.Net.Http;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;
using RTGS.DotNetSDK.Subscriber.Handlers;
using ValidMessages = RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData.ValidMessages;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalHandlers.GivenIdCryptCreateInvitationSentToOpenSubscriberConnection;

public sealed class AndIdCryptCreateInvitationApiIsNotAvailable : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(1_000);

	private readonly List<IHandler> _allTestHandlers = new AllTestHandlers().ToList();

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;

	private StatusCodeHttpHandler _idCryptServiceHttpHandler;
	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;

	public AndIdCryptCreateInvitationApiIsNotAvailable(GrpcServerFixture grpcServer)
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
				.Build();

			_idCryptServiceHttpHandler = StatusCodeHttpHandlerBuilderFactory
				.Create()
				.WithServiceUnavailableResponse(CreateConnectionForBank.Path)
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
			var toRtgsMessageHandler = _grpcServer.Services.GetRequiredService<ToRtgsMessageHandler>();
			toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
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
	public async Task ThenInvitationNotSent()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForReceivedMessageDuration);

		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		receiver.Connections.Should().BeEmpty();
	}

	[Fact]
	public async Task ThenHandlerLogs()
	{
		using var exceptionSignal = new ManualResetEventSlim();

		await _rtgsSubscriber.StartAsync(_allTestHandlers);
		_rtgsSubscriber.OnExceptionOccurred += (_, _) => exceptionSignal.Set();

		await _fromRtgsSender.SendAsync(
			"idcrypt.createinvitation.v1",
			ValidMessages.IdCryptCreateInvitationRequestV1);

		exceptionSignal.Wait();

		using var _ = new AssertionScope();

		var errorLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Error);
		errorLogs.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
			"Error occurred when sending CreateConnectionInvitation request to ID Crypt Service for invitation from bank",
			LogEventLevel.Error,
			typeof(RtgsSubscriberException)));
	}

	[Fact]
	public async Task ThenIdCryptServiceClientLogs()
	{
		using var exceptionSignal = new ManualResetEventSlim();

		await _rtgsSubscriber.StartAsync(_allTestHandlers);
		_rtgsSubscriber.OnExceptionOccurred += (_, _) => exceptionSignal.Set();

		await _fromRtgsSender.SendAsync(
			"idcrypt.createinvitation.v1",
			ValidMessages.IdCryptCreateInvitationRequestV1);

		exceptionSignal.Wait();

		using var _ = new AssertionScope();
		var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.IdCrypt.IdCryptServiceClient", LogEventLevel.Debug);
		debugLogs.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
			"Sending CreateConnectionInvitation request to ID Crypt Service",
			LogEventLevel.Debug));

		var errorLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.IdCrypt.IdCryptServiceClient", LogEventLevel.Error);
		errorLogs.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
				"Error occurred when sending CreateConnectionInvitation request to ID Crypt Service",
				LogEventLevel.Error,
				typeof(HttpRequestException)));
	}
}
