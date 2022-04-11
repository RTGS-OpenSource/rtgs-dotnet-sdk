using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;
using RTGS.DotNetSDK.Subscriber.Handlers;
using ValidMessages = RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData.ValidMessages;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalHandlers.GivenIdCryptBankInvitationSentToOpenSubscriberConnection;

public class AndIdCryptGetConnectionApiIsNotAvailable : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(1_000);

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;
	private readonly List<IHandler> _allTestHandlers = new AllTestHandlers().ToList();

	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;
	private QueueableStatusCodeHttpHandler _idCryptMessageHandler;
	private AllTestHandlers.TestIdCryptBankInvitationNotificationV1 _bankInvitationNotificationHandler;

	public AndIdCryptGetConnectionApiIsNotAvailable(GrpcServerFixture grpcServer)
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
					new Uri("http://id-crypt-cloud-agent-api.com"),
					"id-crypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.Build();

			_idCryptMessageHandler = StatusCodeHttpHandlerBuilderFactory
				.CreateQueueable()
				.WithOkResponse(ReceiveInvitation.HttpRequestResponseContext)
				.WithOkResponse(AcceptInvitation.HttpRequestResponseContext)
				.WithServiceUnavailableResponse(GetConnection.Path)
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices((_, services) => services
					.AddRtgsSubscriber(rtgsSdkOptions)
					.AddTestIdCryptHttpClient(_idCryptMessageHandler))
				.UseSerilog()
				.Build();

			_fromRtgsSender = _grpcServer.Services.GetRequiredService<FromRtgsSender>();
			_rtgsSubscriber = _clientHost.Services.GetRequiredService<IRtgsSubscriber>();
			var toRtgsMessageHandler = _grpcServer.Services.GetRequiredService<ToRtgsMessageHandler>();
			toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			_bankInvitationNotificationHandler = _allTestHandlers.OfType<AllTestHandlers.TestIdCryptBankInvitationNotificationV1>().Single();
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
	public async Task ThenIdCryptInvitationConfirmationNotSent()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync(
			"idcrypt.invitation.tobank.v1",
			ValidMessages.IdCryptBankInvitationV1);

		_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		receiver.Connections.Should().BeEmpty();
	}

	[Fact]
	public async Task ThenLog()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync(
			"idcrypt.invitation.tobank.v1",
			ValidMessages.IdCryptBankInvitationV1);

		_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		var bankDid = ValidMessages.IdCryptBankInvitationV1.FromBankDid;
		var connectionId = ReceiveInvitation.Response.ConnectionId;

		var expectedErrorLogs = new List<LogEntry>
		{
			new("Error occurred when sending GetConnection request to ID Crypt", LogEventLevel.Error, typeof(RtgsSubscriberException)),
			new($"Error occured when polling for connection state for invitation from bank {bankDid}", LogEventLevel.Error, typeof(RtgsSubscriberException)),
		};

		using var _ = new AssertionScope();

		var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptBankInvitationV1Handler", LogEventLevel.Debug);
		debugLogs.Select(log => log.Message)
			.Should().ContainSingle(msg => msg == $"Sending GetConnection request to ID Crypt");

		var errorLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptBankInvitationV1Handler", LogEventLevel.Error);
		errorLogs.Should().BeEquivalentTo(expectedErrorLogs, options => options.WithStrictOrdering());
	}

	[Fact]
	public async Task ThenUserHandlerIsNotInvoked()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync(
			"idcrypt.invitation.tobank.v1",
			ValidMessages.IdCryptBankInvitationV1);

		_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		_bankInvitationNotificationHandler.ReceivedMessage.Should().BeNull();
	}

	[Fact]
	public async Task ThenGetPublicDidIsNotInvoked()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync(
			"idcrypt.invitation.tobank.v1",
			ValidMessages.IdCryptBankInvitationV1);

		_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		_idCryptMessageHandler.Requests
			.ContainsKey(GetPublicDid.Path)
			.Should().BeFalse();
	}
}
