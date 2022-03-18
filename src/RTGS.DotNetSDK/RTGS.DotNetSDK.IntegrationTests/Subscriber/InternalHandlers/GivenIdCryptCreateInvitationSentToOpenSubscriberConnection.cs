using System.Net;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.InternalMessages;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using ValidMessages = RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData.ValidMessages;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalHandlers;

public class GivenIdCryptCreateInvitationSentToOpenSubscriberConnection
{
	public class AndIdCryptApiAvailable : IDisposable, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(5_000);
		private static readonly TimeSpan WaitForSubscriberAcknowledgementDuration = TimeSpan.FromMilliseconds(100);
		private static readonly TimeSpan WaitForPublisherAcknowledgementDuration = TimeSpan.FromMilliseconds(1_000);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;
		private readonly List<IHandler> _allTestHandlers = new AllTestHandlers().ToList();

		private IHost _clientHost;
		private FromRtgsSender _fromRtgsSender;
		private IRtgsSubscriber _rtgsSubscriber;
		private StatusCodeHttpHandler _idCryptMessageHandler;
		private AllTestHandlers.TestIdCryptCreateInvitationNotificationV1 _invitationNotificationHandler;
		private ToRtgsMessageHandler _toRtgsMessageHandler;

		private const string IdCryptApiKey = "id-crypt-api-key";
		private static readonly Uri IdCryptApiUri = new("http://id-crypt-cloud-agent-api.com");

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
			_idCryptMessageHandler = new StatusCodeHttpHandler(IdCryptEndPoints.MockHttpResponses);
			_invitationNotificationHandler = _allTestHandlers.OfType<AllTestHandlers.TestIdCryptCreateInvitationNotificationV1>().Single();

			try
			{
				var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
						ValidMessages.BankDid,
						_grpcServer.ServerUri,
						new Uri("http://id-crypt-cloud-agent-api.com"),
						"id-crypt-api-key",
						new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
					.WaitForAcknowledgementDuration(WaitForPublisherAcknowledgementDuration)
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
		public async Task WhenMessageReceived_ThenSeeBankDidInRequestHeader()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			_fromRtgsSender.RequestHeaders.Should().ContainSingle(header => header.Key == "bankdid" && header.Value == ValidMessages.BankDid);
		}

		[Fact]
		public async Task WhenMessageReceived_ThenPassToHandlerAndAcknowledge()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			var sentRtgsMessage = await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

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

		[Theory]
		[InlineData(IdCryptEndPoints.PublicDidPath)]
		[InlineData(IdCryptEndPoints.InvitationPath)]
		public async Task WhenCallingIdCryptAgent_ThenApiKeyHeaderIsExpected(string path)
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			using var _ = new AssertionScope();

			_idCryptMessageHandler
				.Requests[path]
				.Headers
				.GetValues("X-API-Key")
				.Should().ContainSingle()
				.Which.Should().Be(IdCryptApiKey);
		}

		[Theory]
		[InlineData(IdCryptEndPoints.PublicDidPath)]
		[InlineData(IdCryptEndPoints.InvitationPath)]
		public async Task WhenCallingIdCryptAgent_ThenUriIsExpected(string path)
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			using var _ = new AssertionScope();

			var actualApiUri = _idCryptMessageHandler
				.Requests[path]
				.RequestUri?
				.GetLeftPart(UriPartial.Authority);

			actualApiUri.Should().BeEquivalentTo(IdCryptApiUri.GetLeftPart(UriPartial.Authority));
		}

		[Fact]
		public async Task WhenCallingIdCryptAgent_ThenDefaultQueryParamsAreCorrect()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

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
		public async Task WhenCallingIdCryptAgent_ThenLog()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri?.Query);
			var alias = inviteRequestQueryParams["alias"];

			var expectedLogs = new List<LogEntry>
			{
				new($"Sending CreateInvitation request with alias {alias} to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new($"Sent CreateInvitation request with alias {alias} to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new("Sending GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new("Sent GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new ($"Sending Invitation with alias {alias} to Bank '{ValidMessages.IdCryptCreateInvitationRequestV1.BankPartnerDid}'", LogEventLevel.Debug),
				new ($"Sent Invitation with alias {alias} to Bank '{ValidMessages.IdCryptCreateInvitationRequestV1.BankPartnerDid}'", LogEventLevel.Debug),
			};

			var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Debug);
			debugLogs.Should().BeEquivalentTo(expectedLogs, options => options.WithStrictOrdering());
		}


		[Fact]
		public async Task WhenSubscriberIsStopped_ThenCloseConnection()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

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
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

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
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

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
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			_fromRtgsSender.SetExpectedAcknowledgementCount(2);

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync(
				"cannot be handled",
				ValidMessages.AtomicLockResponseV1);

			await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

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
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _rtgsSubscriber.StopAsync();

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			var sentRtgsMessage = await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

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
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			_fromRtgsSender.SetExpectedAcknowledgementCount(2);

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			_rtgsSubscriber.OnExceptionOccurred += (_, _) => throw new InvalidOperationException("test");

			await _fromRtgsSender.SendAsync("will-throw", ValidMessages.AtomicLockResponseV1);

			await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

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

		[Fact]
		public async Task WhenIdCryptCreateInvitationMessageReceived_ThenIdCryptInvitationMessageIsPublishedToPartnerBank()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

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

	public class AndIdCryptCreateInvitationApiIsNotAvailable : IDisposable, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(1_000);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;
		private readonly List<IHandler> _allTestHandlers = new AllTestHandlers().ToList();

		private IHost _clientHost;
		private FromRtgsSender _fromRtgsSender;
		private IRtgsSubscriber _rtgsSubscriber;
		private StatusCodeHttpHandler _idCryptMessageHandler;
		private AllTestHandlers.TestIdCryptCreateInvitationNotificationV1 _invitationNotificationHandler;

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
						ValidMessages.BankDid,
						_grpcServer.ServerUri,
						new Uri("http://id-crypt-cloud-agent-api.com"),
						"id-crypt-api-key",
						new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
					.Build();

				var mockHttpResponses = new List<MockHttpResponse> {
					new MockHttpResponse {
						Content = new StringContent(IdCryptTestMessages.GetPublicDidResponseJson),
						HttpStatusCode = HttpStatusCode.OK,
						Path = IdCryptEndPoints.PublicDidPath
					},
					new MockHttpResponse {
						Content = null,
						HttpStatusCode = HttpStatusCode.ServiceUnavailable,
						Path = IdCryptEndPoints.InvitationPath
					}
				};

				_idCryptMessageHandler = new StatusCodeHttpHandler(mockHttpResponses);

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

				_invitationNotificationHandler = _allTestHandlers.OfType<AllTestHandlers.TestIdCryptCreateInvitationNotificationV1>().Single();
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

			_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			receiver.Connections.Should().BeEmpty();
		}

		[Fact]
		public async Task ThenLog()
		{
			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri.Query);
			var alias = inviteRequestQueryParams["alias"];

			using var _ = new AssertionScope();
			var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Debug);
			debugLogs.Select(log => log.Message)
				.Should().ContainSingle(msg => msg == $"Sending CreateInvitation request with alias {alias} to ID Crypt Cloud Agent");

			var errorLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Error);
			errorLogs.Select(log => log.Message)
				.Should().ContainSingle(msg => msg == $"Error occurred when sending CreateInvitation request with alias {alias} to ID Crypt Cloud Agent");
		}

		[Fact]
		public async Task ThenUserHandlerIsNotInvoked()
		{
			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			_invitationNotificationHandler.ReceivedMessage.Should().BeNull();
		}
	}

	public class AndIdCryptCreateGetPublicDidApiIsNotAvailable : IDisposable, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(1_000);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;
		private readonly List<IHandler> _allTestHandlers = new AllTestHandlers().ToList();

		private IHost _clientHost;
		private FromRtgsSender _fromRtgsSender;
		private IRtgsSubscriber _rtgsSubscriber;
		private StatusCodeHttpHandler _idCryptMessageHandler;
		private AllTestHandlers.TestIdCryptCreateInvitationNotificationV1 _invitationNotificationHandler;

		public AndIdCryptCreateGetPublicDidApiIsNotAvailable(GrpcServerFixture grpcServer)
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
						ValidMessages.BankDid,
						_grpcServer.ServerUri,
						new Uri("http://id-crypt-cloud-agent-api.com"),
						"id-crypt-api-key",
						new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
					.Build();

				var mockHttpResponses = new List<MockHttpResponse> {
					new MockHttpResponse {
						Content = null,
						HttpStatusCode = HttpStatusCode.ServiceUnavailable,
						Path = IdCryptEndPoints.PublicDidPath
					},
					new MockHttpResponse {
						Content = new StringContent(IdCryptTestMessages.ConnectionInviteResponseJson),
						HttpStatusCode = HttpStatusCode.OK,
						Path = IdCryptEndPoints.InvitationPath
					}
				};

				_idCryptMessageHandler = new StatusCodeHttpHandler(mockHttpResponses);

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

				_invitationNotificationHandler = _allTestHandlers.OfType<AllTestHandlers.TestIdCryptCreateInvitationNotificationV1>().Single();
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

			_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			receiver.Connections.Should().BeEmpty();
		}

		[Fact]
		public async Task ThenLog()
		{
			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri.Query);
			var alias = inviteRequestQueryParams["alias"];

			var expectedDebugLogs = new List<LogEntry>
			{
				new ($"Sending CreateInvitation request with alias {alias} to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new ($"Sent CreateInvitation request with alias {alias} to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new ("Sending GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new ("Sent GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug)
			};

			using var _ = new AssertionScope();

			_serilogContext
				.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Debug)
				.Should().BeEquivalentTo(expectedDebugLogs, options => options.WithStrictOrdering());

			_serilogContext
				.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Error)
				.Should().ContainSingle()
				.Which.Message.Should().Be("Error occurred when sending GetPublicDid request to ID Crypt Cloud Agent");
		}

		[Fact]
		public async Task ThenUserHandlerIsNotInvoked()
		{
			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			_invitationNotificationHandler.ReceivedMessage.Should().BeNull();
		}
	}

	public class AndFailedPublisherConnection : IDisposable, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(1_000);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;
		private readonly List<IHandler> _allTestHandlers = new AllTestHandlers().ToList();

		private AllTestHandlers.TestIdCryptCreateInvitationNotificationV1 _invitationNotificationHandler;
		private IHost _clientHost;
		private FromRtgsSender _fromRtgsSender;
		private IRtgsSubscriber _rtgsSubscriber;
		private StatusCodeHttpHandler _idCryptMessageHandler;

		public AndFailedPublisherConnection(GrpcServerFixture grpcServer)
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
						ValidMessages.BankDid,
						_grpcServer.ServerUri,
						new Uri("http://id-crypt-cloud-agent-api.com"),
						"id-crypt-api-key",
						new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
					.Build();

				_idCryptMessageHandler = new StatusCodeHttpHandler(IdCryptEndPoints.MockHttpResponses);

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services
						.AddRtgsSubscriber(rtgsSdkOptions)
						.AddTestIdCryptHttpClient(_idCryptMessageHandler))
					.UseSerilog()
					.Build();

				_fromRtgsSender = _grpcServer.Services.GetRequiredService<FromRtgsSender>();
				_rtgsSubscriber = _clientHost.Services.GetRequiredService<IRtgsSubscriber>();
				var toRtgsMessageHandler = _grpcServer.Services.GetRequiredService<ToRtgsMessageHandler>();
				toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

				_invitationNotificationHandler = _allTestHandlers.OfType<AllTestHandlers.TestIdCryptCreateInvitationNotificationV1>().Single();
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
		public async Task ThenLog()
		{
			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			receiver.ThrowOnConnection = true;

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

			_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri.Query);
			var alias = inviteRequestQueryParams["alias"];

			var expectedDebugLogs = new List<LogEntry>
			{
				new ($"Sending CreateInvitation request with alias {alias} to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new ($"Sent CreateInvitation request with alias {alias} to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new ("Sending GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new ("Sent GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new ($"Sending Invitation with alias {alias} to Bank '{ValidMessages.IdCryptCreateInvitationRequestV1.BankPartnerDid}'", LogEventLevel.Debug),
			};

			using var _ = new AssertionScope();

			_serilogContext
				.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Debug)
				.Should().BeEquivalentTo(expectedDebugLogs, options => options.WithStrictOrdering());

			_serilogContext
				.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Error)
				.Should().ContainSingle()
				.Which.Message.Should().Be($"Exception occurred when sending IdCrypt Invitation with alias {alias} to Bank '{ValidMessages.IdCryptCreateInvitationRequestV1.BankPartnerDid}'");
		}
	}
}
