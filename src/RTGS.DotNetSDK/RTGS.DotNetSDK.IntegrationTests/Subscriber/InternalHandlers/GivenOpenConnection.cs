using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.Publisher.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalMessages;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using ValidMessages = RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData.ValidMessages;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalHandlers;

public class GivenOpenConnection : IAsyncLifetime, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(5_000);
	private static readonly TimeSpan WaitForAcknowledgementsDuration = TimeSpan.FromMilliseconds(100);

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;
	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;
	private readonly StatusCodeHttpHandler _idCryptMessageHandler;
	private readonly List<IHandler> _allTestHandlers = new AllTestHandlers().ToList();
	private readonly AllTestHandlers.TestIdCryptCreateInvitationNotificationV1 _invitationNotificationHandler;

	private const string IdCryptApiKey = "id-crypt-api-key";
	private static readonly Uri IdCryptApiUri = new("http://id-crypt-cloud-agent-api.com");

	public GivenOpenConnection(GrpcServerFixture grpcServer)
	{
		_grpcServer = grpcServer;

		SetupSerilogLogger();

		_serilogContext = TestCorrelator.CreateContext();

		_idCryptMessageHandler = new StatusCodeHttpHandler(IdCryptEndPoints.MockHttpResponses);
		_invitationNotificationHandler = _allTestHandlers.OfType<AllTestHandlers.TestIdCryptCreateInvitationNotificationV1>().Single();
	}

	private static void SetupSerilogLogger() =>
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Debug()
			.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
			.Enrich.FromLogContext()
			.WriteTo.Console()
			.WriteTo.TestCorrelator()
			.CreateLogger();

	public async Task InitializeAsync()
	{
		try
		{
			var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
					ValidMessages.BankDid,
					_grpcServer.ServerUri,
					new Uri("http://id-crypt-cloud-agent-api.com"),
					"id-crypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
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
		}
		catch (Exception)
		{
			// If an exception occurs then manually clean up as IAsyncLifetime.DisposeAsync is not called.
			// See https://github.com/xunit/xunit/discussions/2313 for further details.
			await DisposeAsync();

			throw;
		}
	}

	public Task DisposeAsync()
	{
		_clientHost?.Dispose();

		_grpcServer.Reset();

		return Task.CompletedTask;
	}

	[Fact]
	public async Task WhenUsingMetadata_ThenSeeBankDidInRequestHeader()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		_fromRtgsSender.RequestHeaders.Should().ContainSingle(header => header.Key == "bankdid" && header.Value == ValidMessages.BankDid);
	}

	[Fact]
	public async Task WhenIdCryptCreateInvitationMessageReceived_ThenPassToHandlerAndAcknowledge()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		var sentRtgsMessage = await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		using var _ = new AssertionScope();

		_fromRtgsSender.Acknowledgements
			.Should().ContainSingle(acknowledgement => acknowledgement.CorrelationId == sentRtgsMessage.CorrelationId
													   && acknowledgement.Success);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri?.Query);
		var alias = inviteRequestQueryParams["alias"];

		var message = new IdCryptCreateInvitationNotificationV1
		{
			Alias = alias,
			ConnectionId = IdCryptTestMessages.ConnectionInviteResponse.ConnectionID,
			BankPartnerDid = ValidMessages.IdCryptCreateInvitationRequestV1.BankPartnerDid
		};

		_invitationNotificationHandler.ReceivedMessage.Should().BeEquivalentTo(message);
	}

	[Fact]
	public async Task WhenCallingIdCryptAgent_ThenApiKeyHeaderIsExpected()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		using var _ = new AssertionScope();

		var actualApiKey = _idCryptMessageHandler
			.Requests[IdCryptEndPoints.InvitationPath]
			.Headers
			.GetValues("X-API-Key")
			.Single();

		actualApiKey.Should().Be(IdCryptApiKey);
	}

	[Fact]
	public async Task WhenCallingIdCryptAgent_ThenUriIsExpected()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		using var _ = new AssertionScope();

		var actualApiUri = _idCryptMessageHandler
			.Requests[IdCryptEndPoints.InvitationPath]
			.RequestUri?
			.GetLeftPart(UriPartial.Authority);

		actualApiUri.Should().BeEquivalentTo(IdCryptApiUri.GetLeftPart(UriPartial.Authority));
	}

	[Fact]
	public async Task WhenCallingIdCryptAgent_ThenDefaultQueryParamsAreCorrect()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri?.Query);
		var autoAccept = bool.Parse(inviteRequestQueryParams["auto_accept"]);
		var multiUse = bool.Parse(inviteRequestQueryParams["multi_use"]);
		var usePublicDid = bool.Parse(inviteRequestQueryParams["public"]);

		using var _ = new AssertionScope();

		autoAccept.Should().BeTrue();
		multiUse.Should().BeFalse();
		usePublicDid.Should().BeFalse();
	}

	[Fact]
	public async Task WhenIdCryptCreateInvitationMessageReceived_ThenLogDebug()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri?.Query);
		var alias = inviteRequestQueryParams["alias"];

		var expectedLogs = new List<LogEntry>
		{
			new($"Sending CreateInvitation request with alias {alias} to ID Crypt Cloud Agent", LogEventLevel.Debug),
			new("Sent CreateInvitation request to ID Crypt Cloud Agent", LogEventLevel.Debug),
			new("Sending GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug),
			new("Sent GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug),
		};

		var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Debug);
		debugLogs.Should().BeEquivalentTo(expectedLogs, options => options.WithStrictOrdering());
	}


	[Fact]
	public async Task WhenSubscriberIsStopped_ThenCloseConnection()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		await _rtgsSubscriber.StopAsync();

		_invitationNotificationHandler.Reset();

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		_invitationNotificationHandler.ReceivedMessage.Should().BeNull();
	}

	[Fact]
	public async Task WhenSubscriberIsDisposed_ThenCloseConnection()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		await _rtgsSubscriber.DisposeAsync();

		_invitationNotificationHandler.Reset();

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		_invitationNotificationHandler.ReceivedMessage.Should().BeNull();
	}

	[Fact]
	public async Task WhenIdCryptCreateInvitationMessageReceived_ThenLogInformation()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

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
		_fromRtgsSender.SetExpectedAcknowledgementCount(2);

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync(
			"cannot be handled",
			ValidMessages.AtomicLockResponseV1);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri?.Query);
		var alias = inviteRequestQueryParams["alias"];

		var message = new IdCryptCreateInvitationNotificationV1
		{
			Alias = alias,
			ConnectionId = IdCryptTestMessages.ConnectionInviteResponse.ConnectionID,
			BankPartnerDid = ValidMessages.IdCryptCreateInvitationRequestV1.BankPartnerDid
		};

		_invitationNotificationHandler.ReceivedMessage.Should().BeEquivalentTo(message);

		await _rtgsSubscriber.StopAsync();
	}

	[Fact]
	public async Task AndSubscriberIsStopped_WhenStarting_ThenReceiveMessages()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _rtgsSubscriber.StopAsync();

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		var sentRtgsMessage = await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		using var _ = new AssertionScope();

		_fromRtgsSender.Acknowledgements
			.Should().ContainSingle(acknowledgement => acknowledgement.CorrelationId == sentRtgsMessage.CorrelationId
													   && acknowledgement.Success);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri?.Query);
		var alias = inviteRequestQueryParams["alias"];

		var message = new IdCryptCreateInvitationNotificationV1
		{
			Alias = alias,
			ConnectionId = IdCryptTestMessages.ConnectionInviteResponse.ConnectionID,
			BankPartnerDid = ValidMessages.IdCryptCreateInvitationRequestV1.BankPartnerDid
		};

		_invitationNotificationHandler.ReceivedMessage.Should().BeEquivalentTo(message);
	}

	[Fact]
	public async Task WhenExceptionEventHandlerThrows_ThenSubsequentMessagesCanBeHandled()
	{
		_fromRtgsSender.SetExpectedAcknowledgementCount(2);

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		_rtgsSubscriber.OnExceptionOccurred += (_, _) => throw new InvalidOperationException("test");

		await _fromRtgsSender.SendAsync("will-throw", ValidMessages.AtomicLockResponseV1);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri?.Query);
		var alias = inviteRequestQueryParams["alias"];

		var message = new IdCryptCreateInvitationNotificationV1
		{
			Alias = alias,
			ConnectionId = IdCryptTestMessages.ConnectionInviteResponse.ConnectionID,
			BankPartnerDid = ValidMessages.IdCryptCreateInvitationRequestV1.BankPartnerDid
		};

		_invitationNotificationHandler.ReceivedMessage.Should().BeEquivalentTo(message);
	}

	[Fact]
	public async Task AndMessageIsBeingProcessed_WhenStopping_ThenHandleGracefully()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		await _rtgsSubscriber.StopAsync();

		_serilogContext.SubscriberLogs(LogEventLevel.Error).Should().BeEmpty();
	}

	[Fact]
	public async Task AndMessageIsBeingProcessed_WhenDisposing_ThenHandleGracefully()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		await _rtgsSubscriber.DisposeAsync();

		_serilogContext.SubscriberLogs(LogEventLevel.Error).Should().BeEmpty();
	}

	[Fact]
	public async Task WhenIdCryptCreateInvitationMessageReceived_ThenIdCryptInvitationMessageIsPublishedToPartnerBank()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();
		var receivedMessage = receiver.Connections
			.Should().ContainSingle().Which.Requests
			.Should().ContainSingle().Subject;

		using var _ = new AssertionScope();

		receivedMessage.MessageIdentifier.Should().Be("idcrypt.invitation.tobank.v1");
		receivedMessage.CorrelationId.Should().NotBeNullOrEmpty();

		var inviteRequestQueryParams = QueryHelpers.ParseQuery(
			_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri?.Query);

		var invitation = IdCryptTestMessages.ConnectionInviteResponse.Invitation;
		var agentPublicDid = IdCryptTestMessages.GetPublicDidResponse.Result.DID;

		var expectedMessageData = new IdCryptInvitationV1
		{
			Alias = inviteRequestQueryParams["alias"],
			Label = invitation.Label,
			RecipientKeys = invitation.RecipientKeys,
			Id = invitation.ID,
			Type = invitation.Type,
			ServiceEndPoint = invitation.ServiceEndPoint,
			AgentPublicDid = agentPublicDid
		};

		var actualMessageData = JsonSerializer
			.Deserialize<IdCryptInvitationV1>(receivedMessage.Data);

		actualMessageData.Should().BeEquivalentTo(expectedMessageData);
	}
}
