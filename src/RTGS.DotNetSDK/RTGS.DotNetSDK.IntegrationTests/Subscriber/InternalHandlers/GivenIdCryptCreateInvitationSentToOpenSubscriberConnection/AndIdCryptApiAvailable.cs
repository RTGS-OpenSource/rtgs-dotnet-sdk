using System.Text.Json;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.IDCrypt.Service.Contracts.Connection;
using ValidMessages = RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData.ValidMessages;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalHandlers.GivenIdCryptCreateInvitationSentToOpenSubscriberConnection;

public sealed class AndIdCryptApiAvailable : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan WaitForReceivedRequestDuration = TimeSpan.FromMilliseconds(5_000);
	private static readonly TimeSpan WaitForSubscriberAcknowledgementDuration = TimeSpan.FromMilliseconds(100);
	private static readonly TimeSpan WaitForPublisherAcknowledgementDuration = TimeSpan.FromMilliseconds(1_000);
	private static readonly Uri IdCryptServiceUri = new("https://id-crypt-service");

	private readonly List<IHandler> _allTestHandlers = new AllTestHandlers().ToList();

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;

	private StatusCodeHttpHandler _idCryptServiceHttpHandler;
	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;
	private ToRtgsMessageHandler _toRtgsMessageHandler;

	public AndIdCryptApiAvailable(GrpcServerFixture grpcServer)
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
		_idCryptServiceHttpHandler = StatusCodeHttpHandlerBuilderFactory
			.Create()
			.WithOkResponse(CreateConnectionForBank.HttpRequestResponseContext)
			.Build();

		try
		{
			var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
					ValidMessages.RtgsGlobalId,
					_grpcServer.ServerUri,
					IdCryptServiceUri)
				.WaitForAcknowledgementDuration(WaitForPublisherAcknowledgementDuration)
				.EnableMessageSigning()
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
	public async Task WhenCallingIdCryptService_ThenContentIsExpected()
	{
		_toRtgsMessageHandler.SetupForMessage(handler =>
			handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_idCryptServiceHttpHandler.WaitForRequests(WaitForReceivedRequestDuration);

		var actualContent = await _idCryptServiceHttpHandler.Requests[CreateConnectionForBank.Path]
			.Single().Content!.ReadAsStringAsync();

		var actualRequest = JsonSerializer.Deserialize<CreateConnectionInvitationForBankRequest>(actualContent);

		var expectedRequest = new CreateConnectionInvitationForBankRequest
		{
			RtgsGlobalId = "RTGS:GB177550GB"
		};

		actualRequest.Should().BeEquivalentTo(expectedRequest);
	}

	[Fact]
	public async Task WhenMessageReceived_ThenSeeRtgsGlobalIdInRequestHeader()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_idCryptServiceHttpHandler.WaitForRequests(WaitForReceivedRequestDuration);

		_fromRtgsSender.RequestHeaders.Should().ContainSingle(header =>
			header.Key == "rtgs-global-id"
			&& header.Value == ValidMessages.RtgsGlobalId);
	}

	[Fact]
	public async Task WhenMessageReceived_ThenAcknowledge()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		var sentRtgsMessage = await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		using var _ = new AssertionScope();

		_fromRtgsSender.Acknowledgements.Should().ContainSingle(acknowledgement =>
			acknowledgement.CorrelationId == sentRtgsMessage.CorrelationId
			&& acknowledgement.Success);
	}

	[Fact]
	public async Task WhenCallingIdCryptService_ThenUriIsExpected()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_idCryptServiceHttpHandler.WaitForRequests(WaitForReceivedRequestDuration);

		using var _ = new AssertionScope();

		var actualApiUri = _idCryptServiceHttpHandler
			.Requests[CreateConnectionForBank.Path]
			.Single()
			.RequestUri!
			.GetLeftPart(UriPartial.Authority);

		actualApiUri.Should().BeEquivalentTo(IdCryptServiceUri.GetLeftPart(UriPartial.Authority));
	}

	[Fact]
	public async Task WhenCallingIdCryptService_ThenHandlerLogs()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		_idCryptServiceHttpHandler.WaitForRequests(WaitForReceivedRequestDuration);

		var alias = CreateConnectionForBank.Response.Alias;
		var bankPartnerRtgsGlobalId = ValidMessages.IdCryptCreateInvitationRequestV1.BankPartnerRtgsGlobalId;

		var expectedLogs = new List<LogEntry>
		{
			new ($"Sending Invitation with alias {alias} to Bank {bankPartnerRtgsGlobalId}", LogEventLevel.Debug),
			new ($"Sent Invitation with alias {alias} to Bank {bankPartnerRtgsGlobalId}", LogEventLevel.Debug),
		};

		var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Debug);

		Action assert = () => debugLogs.Should().BeEquivalentTo(expectedLogs, options => options.WithStrictOrdering());
		assert.Within(500);
	}

	[Fact]
	public async Task WhenCallingIdCryptService_ThenIdCryptServiceClientLogs()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		_idCryptServiceHttpHandler.WaitForRequests(WaitForReceivedRequestDuration);

		var expectedLogs = new List<LogEntry>
		{
			new("Sending create connection invitation for bank request to ID Crypt Service", LogEventLevel.Debug),
			new("Sent create connection invitation for bank request to ID Crypt Service", LogEventLevel.Debug)
		};

		var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.IdCrypt.IdCryptServiceClient", LogEventLevel.Debug);

		Action assert = () => debugLogs.Should().BeEquivalentTo(expectedLogs, options => options.WithStrictOrdering());
		assert.Within(500);
	}

	[Fact]
	public async Task WhenSubscriberIsStopped_ThenCloseConnection()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_idCryptServiceHttpHandler.WaitForRequests(WaitForReceivedRequestDuration);

		await _rtgsSubscriber.StopAsync();

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.Acknowledgements.Should().HaveCount(1);
	}

	[Fact]
	public async Task WhenSubscriberIsDisposed_ThenCloseConnection()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		_idCryptServiceHttpHandler.WaitForRequests(WaitForReceivedRequestDuration);

		await _rtgsSubscriber.DisposeAsync();

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.Acknowledgements.Should().HaveCount(1);
	}

	[Fact]
	public async Task WhenIdCryptCreateInvitationMessageReceived_ThenLogInformation()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		_idCryptServiceHttpHandler.WaitForRequests(WaitForReceivedRequestDuration);

		await _rtgsSubscriber.StopAsync();

		var informationLogs = _serilogContext.SubscriberLogs(LogEventLevel.Information);
		var expectedLogs = new List<LogEntry>
		{
			new("RTGS Subscriber started", LogEventLevel.Information),
			new("idcrypt.createinvitation.v1 message received from RTGS", LogEventLevel.Information),
			new("RTGS Subscriber stopping", LogEventLevel.Information),
			new("RTGS Subscriber stopped", LogEventLevel.Information)
		};

		informationLogs.Should().BeEquivalentTo(expectedLogs, options => options.WithStrictOrdering());
	}

	[Fact]
	public async Task WhenMessageWithIdentifierThatCannotBeHandledReceived_ThenSubsequentMessagesCanBeHandled()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		_fromRtgsSender.SetExpectedAcknowledgementCount(2);

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync(
			"cannot be handled",
			ValidMessages.AtomicLockResponseV1);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		_idCryptServiceHttpHandler.WaitForRequests(WaitForReceivedRequestDuration);

		await _rtgsSubscriber.StopAsync();

		var alias = CreateConnectionForBank.Response.Alias;
		var bankPartnerRtgsGlobalId = ValidMessages.IdCryptCreateInvitationRequestV1.BankPartnerRtgsGlobalId;

		var expectedDebugLogs = new List<LogEntry>
		{
			new ($"Sending Invitation with alias {alias} to Bank {bankPartnerRtgsGlobalId}", LogEventLevel.Debug),
			new($"Sent Invitation with alias {alias} to Bank {bankPartnerRtgsGlobalId}", LogEventLevel.Debug)
		};

		var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Debug);

		Action assert = () => debugLogs.Should().BeEquivalentTo(expectedDebugLogs, options => options.WithStrictOrdering());
		assert.Within(500);
	}

	[Fact]
	public async Task AndSubscriberIsStopped_WhenStarting_ThenReceiveMessages()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _rtgsSubscriber.StopAsync();

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		var sentRtgsMessage = await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		_fromRtgsSender.Acknowledgements
			.Should().ContainSingle(acknowledgement => acknowledgement.CorrelationId == sentRtgsMessage.CorrelationId
													   && acknowledgement.Success);
	}

	[Fact]
	public async Task WhenExceptionEventHandlerThrows_ThenSubsequentMessagesCanBeHandled()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		_fromRtgsSender.SetExpectedAcknowledgementCount(2);

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		_rtgsSubscriber.OnExceptionOccurred += (_, _) => throw new InvalidOperationException("test");

		await _fromRtgsSender.SendAsync("will-throw", ValidMessages.AtomicLockResponseV1);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		_idCryptServiceHttpHandler.WaitForRequests(WaitForReceivedRequestDuration);

		await _rtgsSubscriber.StopAsync();

		var alias = CreateConnectionForBank.Response.Alias;
		var bankPartnerRtgsGlobalId = ValidMessages.IdCryptCreateInvitationRequestV1.BankPartnerRtgsGlobalId;

		var expectedDebugLogs = new List<LogEntry>
		{
			new ($"Sending Invitation with alias {alias} to Bank {bankPartnerRtgsGlobalId}", LogEventLevel.Debug),
			new($"Sent Invitation with alias {alias} to Bank {bankPartnerRtgsGlobalId}", LogEventLevel.Debug)
		};

		var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Debug);

		Action assert = () => debugLogs.Should().BeEquivalentTo(expectedDebugLogs, options => options.WithStrictOrdering());
		assert.Within(500);
	}

	[Fact]
	public async Task AndMessageIsBeingProcessed_WhenStopping_ThenHandleGracefully()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		await _rtgsSubscriber.StopAsync();

		_serilogContext.SubscriberLogs(LogEventLevel.Error).Should().BeEmpty();
	}

	[Fact]
	public async Task AndMessageIsBeingProcessed_WhenDisposing_ThenHandleGracefully()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		await _rtgsSubscriber.DisposeAsync();

		_serilogContext.SubscriberLogs(LogEventLevel.Error).Should().BeEmpty();
	}
}
