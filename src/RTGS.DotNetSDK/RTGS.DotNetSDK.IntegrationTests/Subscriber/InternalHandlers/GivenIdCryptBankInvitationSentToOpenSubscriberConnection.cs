﻿using System.Text.Json;
using IDCryptGlobal.Cloud.Agent.Identity.Connection;
using Microsoft.AspNetCore.WebUtilities;
using RTGS.DotNetSDK.IdCrypt.Messages;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.Subscriber.Handlers;
using ValidMessages = RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData.ValidMessages;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalHandlers;

public class GivenIdCryptBankInvitationSentToOpenSubscriberConnection
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
		private AllTestHandlers.TestIdCryptBankInvitationNotificationV1 _bankInvitationNotificationHandler;
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
			_bankInvitationNotificationHandler = _allTestHandlers
				.OfType<AllTestHandlers.TestIdCryptBankInvitationNotificationV1>().Single();

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

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			_fromRtgsSender.RequestHeaders.Should().ContainSingle(header => header.Key == "bankdid" && header.Value == ValidMessages.BankDid);
		}

		[Fact]
		public async Task WhenMessageReceived_ThenPassToHandlerAndAcknowledge()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			var sentRtgsMessage = await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			using var _ = new AssertionScope();

			_fromRtgsSender.Acknowledgements
				.Should().ContainSingle(acknowledgement => acknowledgement.CorrelationId == sentRtgsMessage.CorrelationId
														   && acknowledgement.Success);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var receiveInvitationRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.ReceiveInvitationPath].RequestUri?.Query);
			var alias = receiveInvitationRequestQueryParams["alias"];

			var message = new IdCryptBankInvitationNotificationV1
			{
				BankPartnerDid = ValidMessages.IdCryptBankInvitationV1.FromBankDid,
				Alias = alias,
				ConnectionId = IdCryptTestMessages.ReceiveInvitationResponse.ConnectionID
			};

			_bankInvitationNotificationHandler.ReceivedMessage.Should().BeEquivalentTo(message);
		}

		[Theory]
		[InlineData(IdCryptEndPoints.ReceiveInvitationPath)]
		[InlineData(IdCryptEndPoints.AcceptInvitationPath)]
		[InlineData(IdCryptEndPoints.GetConnectionPath)]
		public async Task WhenCallingIdCryptAgent_ThenApiKeyHeaderIsExpected(string path)
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			using var _ = new AssertionScope();

			_idCryptMessageHandler
				.Requests[path]
				.Headers
				.GetValues("X-API-Key")
				.Should().ContainSingle()
				.Which.Should().Be(IdCryptApiKey);
		}

		[Theory]
		[InlineData(IdCryptEndPoints.ReceiveInvitationPath)]
		[InlineData(IdCryptEndPoints.AcceptInvitationPath)]
		[InlineData(IdCryptEndPoints.GetConnectionPath)]
		public async Task WhenCallingIdCryptAgent_ThenUriIsExpected(string path)
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			using var _ = new AssertionScope();

			var actualApiUri = _idCryptMessageHandler
				.Requests[path]
				.RequestUri?
				.GetLeftPart(UriPartial.Authority);

			actualApiUri.Should().BeEquivalentTo(IdCryptApiUri.GetLeftPart(UriPartial.Authority));
		}

		[Fact]
		public async Task WhenCallingIdCryptAgent_ThenRequestBodyIsExpected()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var content = await _idCryptMessageHandler.Requests[IdCryptEndPoints.ReceiveInvitationPath].Content
				!.ReadAsStringAsync();

			var actualRequestBody = Newtonsoft.Json.JsonConvert.DeserializeObject<ConnectionInvite>(content);

			var invitation = ValidMessages.IdCryptBankInvitationV1.Invitation;
			var expectedRequestBody = new ConnectionInvite
			{
				Alias = invitation.Alias,
				Label = invitation.Label,
				RecipientKeys = invitation.RecipientKeys.ToArray(),
				ID = invitation.Id,
				Type = invitation.Type,
				ServiceEndPoint = invitation.ServiceEndPoint
			};

			actualRequestBody.Should().BeEquivalentTo(expectedRequestBody);
		}

		[Fact]
		public async Task WhenCallingIdCryptAgent_ThenLog()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var bankDid = ValidMessages.IdCryptBankInvitationV1.FromBankDid;
			var connectionId = IdCryptTestMessages.ReceiveInvitationResponse.ConnectionID;

			var expectedLogs = new List<LogEntry>
			{
				new($"Sending ReceiveAcceptInvitation request to ID Crypt for invitation from bank '{bankDid}'", LogEventLevel.Debug),
				new($"ID Crypt invitation from bank '{bankDid}' accepted", LogEventLevel.Debug),
				new($"Polling for connection '{connectionId}' state for invitation from bank '{bankDid}'", LogEventLevel.Debug)
			};

			var infoLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptBankInvitationV1Handler", LogEventLevel.Debug);
			infoLogs.Should().BeEquivalentTo(expectedLogs, options => options.WithStrictOrdering());
		}

		[Fact]
		public async Task WhenSubscriberIsStopped_ThenCloseConnection()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			await _rtgsSubscriber.StopAsync();

			_bankInvitationNotificationHandler.Reset();

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			_bankInvitationNotificationHandler.ReceivedMessage.Should().BeNull();
		}

		[Fact]
		public async Task WhenSubscriberIsDisposed_ThenCloseConnection()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			await _rtgsSubscriber.DisposeAsync();

			_bankInvitationNotificationHandler.Reset();

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			_bankInvitationNotificationHandler.ReceivedMessage.Should().BeNull();
		}

		[Fact]
		public async Task WhenIdCryptBankInvitationMessageReceived_ThenLogInformation()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			await _rtgsSubscriber.StopAsync();

			var informationLogs = _serilogContext.SubscriberLogs(LogEventLevel.Information);
			var expectedLogs = new List<LogEntry>
			{
				new("RTGS Subscriber started", LogEventLevel.Information),
				new("idcrypt.invitation.tobank.v1 message received from RTGS", LogEventLevel.Information),
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

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var receiveInvitationRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.ReceiveInvitationPath].RequestUri?.Query);
			var alias = receiveInvitationRequestQueryParams["alias"];

			var message = new IdCryptBankInvitationNotificationV1
			{
				BankPartnerDid = ValidMessages.IdCryptBankInvitationV1.FromBankDid,
				Alias = alias,
				ConnectionId = IdCryptTestMessages.ReceiveInvitationResponse.ConnectionID
			};

			_bankInvitationNotificationHandler.ReceivedMessage.Should().BeEquivalentTo(message);

			await _rtgsSubscriber.StopAsync();
		}

		[Fact]
		public async Task AndSubscriberIsStopped_WhenStarting_ThenReceiveMessages()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _rtgsSubscriber.StopAsync();

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			var sentRtgsMessage = await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			using var _ = new AssertionScope();

			_fromRtgsSender.Acknowledgements
				.Should().ContainSingle(acknowledgement => acknowledgement.CorrelationId == sentRtgsMessage.CorrelationId
														   && acknowledgement.Success);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var receiveInvitationRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.ReceiveInvitationPath].RequestUri?.Query);
			var alias = receiveInvitationRequestQueryParams["alias"];

			var message = new IdCryptBankInvitationNotificationV1
			{
				BankPartnerDid = ValidMessages.IdCryptBankInvitationV1.FromBankDid,
				Alias = alias,
				ConnectionId = IdCryptTestMessages.ReceiveInvitationResponse.ConnectionID
			};

			_bankInvitationNotificationHandler.ReceivedMessage.Should().BeEquivalentTo(message);
		}

		[Fact]
		public async Task WhenExceptionEventHandlerThrows_ThenSubsequentMessagesCanBeHandled()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			_fromRtgsSender.SetExpectedAcknowledgementCount(2);

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			_rtgsSubscriber.OnExceptionOccurred += (_, _) => throw new InvalidOperationException("test");

			await _fromRtgsSender.SendAsync("will-throw", ValidMessages.AtomicLockResponseV1);

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var receiveInvitationRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.ReceiveInvitationPath].RequestUri?.Query);
			var alias = receiveInvitationRequestQueryParams["alias"];

			var message = new IdCryptBankInvitationNotificationV1
			{
				BankPartnerDid = ValidMessages.IdCryptBankInvitationV1.FromBankDid,
				Alias = alias,
				ConnectionId = IdCryptTestMessages.ReceiveInvitationResponse.ConnectionID
			};

			_bankInvitationNotificationHandler.ReceivedMessage.Should().BeEquivalentTo(message);
		}

		[Fact]
		public async Task AndMessageIsBeingProcessed_WhenStopping_ThenHandleGracefully()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			await _rtgsSubscriber.StopAsync();

			_serilogContext.SubscriberLogs(LogEventLevel.Error).Should().BeEmpty();
		}

		[Fact]
		public async Task AndMessageIsBeingProcessed_WhenDisposing_ThenHandleGracefully()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			await _rtgsSubscriber.DisposeAsync();

			_serilogContext.SubscriberLogs(LogEventLevel.Error).Should().BeEmpty();
		}

		[Fact]
		public async Task WhenIdCryptBankInvitationMessageReceived_ThenIdCryptInvitationConfirmationMessageIsPublishedToPartnerBank()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();
			var receivedMessage = receiver.Connections
				.Should().ContainSingle().Which.Requests
				.Should().ContainSingle().Subject;

			using var _ = new AssertionScope();

			receivedMessage.MessageIdentifier.Should().Be("idcrypt.invitation.tobank.confirmation.v1");
			receivedMessage.CorrelationId.Should().NotBeNullOrEmpty();

			var receiveInvitationRequestQueryParams = QueryHelpers.ParseQuery(
				_idCryptMessageHandler.Requests[IdCryptEndPoints.ReceiveInvitationPath].RequestUri?.Query);

			var expectedMessageData = new IdCryptInvitationConfirmationV1
			{
				Alias = receiveInvitationRequestQueryParams["alias"]
			};

			var actualMessageData = JsonSerializer.Deserialize<IdCryptInvitationConfirmationV1>(receivedMessage.Data);

			actualMessageData.Should().BeEquivalentTo(expectedMessageData);
		}
	}
}
