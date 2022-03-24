﻿using System.Text.Json;
using IDCryptGlobal.Cloud.Agent.Identity.Connection;
using Microsoft.AspNetCore.WebUtilities;
using RTGS.DotNetSDK.IdCrypt.Messages;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;
using RTGS.DotNetSDK.Subscriber.Handlers;
using ValidMessages = RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData.ValidMessages;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalHandlers.GivenIdCryptBankInvitationSentToOpenSubscriberConnection;

public class AndIdCryptApiAvailable
{
	public class WhenGetConnectionPolledOnce : IDisposable, IClassFixture<GrpcServerFixture>
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

		public WhenGetConnectionPolledOnce(GrpcServerFixture grpcServer)
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
			_idCryptMessageHandler = StatusCodeHttpHandlerBuilder
				.Create()
				.WithOkResponse(ReceiveInvitation.HttpRequestResponseContext)
				.WithOkResponse(AcceptInvitation.HttpRequestResponseContext)
				.WithOkResponse(GetConnection.HttpRequestResponseContext)
				.WithOkResponse(GetPublicDid.HttpRequestResponseContext)
				.Build();

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

			var receiveInvitationRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler
				.Requests[ReceiveInvitation.Path].Single().RequestUri!.Query);
			var alias = receiveInvitationRequestQueryParams["alias"];

			var message = new IdCryptBankInvitationNotificationV1
			{
				BankPartnerDid = ValidMessages.IdCryptBankInvitationV1.FromBankDid,
				Alias = alias,
				ConnectionId = ReceiveInvitation.Response.ConnectionID
			};

			_bankInvitationNotificationHandler.ReceivedMessage.Should().BeEquivalentTo(message);
		}

		[Theory]
		[InlineData(ReceiveInvitation.Path)]
		[InlineData(AcceptInvitation.Path)]
		[InlineData(GetConnection.Path)]
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
				.Single()
				.Headers
				.GetValues("X-API-Key")
				.Should().ContainSingle()
				.Which.Should().Be(IdCryptApiKey);
		}

		[Theory]
		[InlineData(ReceiveInvitation.Path)]
		[InlineData(AcceptInvitation.Path)]
		[InlineData(GetConnection.Path)]
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
				.Single()
				.RequestUri!
				.GetLeftPart(UriPartial.Authority);

			actualApiUri.Should().BeEquivalentTo(IdCryptApiUri.GetLeftPart(UriPartial.Authority));
		}

		[Fact]
		public async Task WhenCallingIdCryptAgentReceiveInvite_ThenRequestBodyIsExpected()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var content =
				await _idCryptMessageHandler.Requests[ReceiveInvitation.Path].Single().Content!
					.ReadAsStringAsync();

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
			var connectionId = ReceiveInvitation.Response.ConnectionID;

			var expectedLogs = new List<LogEntry>
			{
				new($"Sending ReceiveAcceptInvitation request to ID Crypt for invitation from bank '{bankDid}'", LogEventLevel.Debug),
				new($"Sent ReceiveAcceptInvitation request to ID Crypt for invitation from bank '{bankDid}'", LogEventLevel.Debug),
				new($"Polling for connection '{connectionId}' state for invitation from bank '{bankDid}'", LogEventLevel.Debug),
				new($"Sending GetConnection request to ID Crypt with connection Id '{connectionId}'", LogEventLevel.Debug),
				new($"Sent GetConnection request to ID Crypt with connection Id '{connectionId}'", LogEventLevel.Debug),
				new($"Finished polling for connection '{connectionId}' state for invitation from bank '{bankDid}'", LogEventLevel.Debug),
				new("Sending GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new("Sent GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new($"Sending ID Crypt invitation confirmation to bank '{bankDid}'", LogEventLevel.Debug),
				new($"Sent ID Crypt invitation confirmation to bank '{bankDid}'", LogEventLevel.Debug)
			};

			var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptBankInvitationV1Handler", LogEventLevel.Debug);
			debugLogs.Should().BeEquivalentTo(expectedLogs, options => options.WithStrictOrdering());
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

			var receiveInvitationRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler
				.Requests[ReceiveInvitation.Path].Single().RequestUri!.Query);
			var alias = receiveInvitationRequestQueryParams["alias"];

			var message = new IdCryptBankInvitationNotificationV1
			{
				BankPartnerDid = ValidMessages.IdCryptBankInvitationV1.FromBankDid,
				Alias = alias,
				ConnectionId = ReceiveInvitation.Response.ConnectionID
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

			var receiveInvitationRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler
				.Requests[ReceiveInvitation.Path].Single().RequestUri!.Query);
			var alias = receiveInvitationRequestQueryParams["alias"];

			var message = new IdCryptBankInvitationNotificationV1
			{
				BankPartnerDid = ValidMessages.IdCryptBankInvitationV1.FromBankDid,
				Alias = alias,
				ConnectionId = ReceiveInvitation.Response.ConnectionID
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

			var receiveInvitationRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler
				.Requests[ReceiveInvitation.Path].Single().RequestUri!.Query);
			var alias = receiveInvitationRequestQueryParams["alias"];

			var message = new IdCryptBankInvitationNotificationV1
			{
				BankPartnerDid = ValidMessages.IdCryptBankInvitationV1.FromBankDid,
				Alias = alias,
				ConnectionId = ReceiveInvitation.Response.ConnectionID
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

			receivedMessage.MessageIdentifier.Should().Be("idcrypt.invitationconfirmation.v1");
			receivedMessage.CorrelationId.Should().NotBeNullOrEmpty();

			var receiveInvitationRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler
				.Requests[ReceiveInvitation.Path].Single().RequestUri!.Query);

			var expectedMessageData = new IdCryptInvitationConfirmationV1
			{
				Alias = receiveInvitationRequestQueryParams["alias"],
				AgentPublicDid = GetPublicDid.Response.Result.DID
			};

			var actualMessageData = JsonSerializer.Deserialize<IdCryptInvitationConfirmationV1>(receivedMessage.Data);

			actualMessageData.Should().BeEquivalentTo(expectedMessageData);
		}
	}

	public class WhenGetConnectionPolledMultipleTimes : IDisposable, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(20_000);
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

		public WhenGetConnectionPolledMultipleTimes(GrpcServerFixture grpcServer)
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
			_idCryptMessageHandler = StatusCodeHttpHandlerBuilder
				.Create()
				.WithOkResponse(ReceiveInvitation.HttpRequestResponseContext)
				.WithOkResponse(AcceptInvitation.HttpRequestResponseContext)
				.WithOkResponse(GetConnection.HttpRequestResponseContextWithState("request"))
				.WithOkResponse(GetConnection.HttpRequestResponseContextWithState("active"))
				.WithOkResponse(GetPublicDid.HttpRequestResponseContext)
				.Build();

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
		public async Task WhenCallingIdCryptAgent_ThenLog()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var bankDid = ValidMessages.IdCryptBankInvitationV1.FromBankDid;
			var connectionId = ReceiveInvitation.Response.ConnectionID;

			var expectedLogs = new List<LogEntry>
			{
				new($"Sending ReceiveAcceptInvitation request to ID Crypt for invitation from bank '{bankDid}'", LogEventLevel.Debug),
				new($"Sent ReceiveAcceptInvitation request to ID Crypt for invitation from bank '{bankDid}'", LogEventLevel.Debug),
				new($"Polling for connection '{connectionId}' state for invitation from bank '{bankDid}'", LogEventLevel.Debug),
				new($"Sending GetConnection request to ID Crypt with connection Id '{connectionId}'", LogEventLevel.Debug),
				new($"Sent GetConnection request to ID Crypt with connection Id '{connectionId}'", LogEventLevel.Debug),
				new($"Sending GetConnection request to ID Crypt with connection Id '{connectionId}'", LogEventLevel.Debug),
				new($"Sent GetConnection request to ID Crypt with connection Id '{connectionId}'", LogEventLevel.Debug),
				new($"Finished polling for connection '{connectionId}' state for invitation from bank '{bankDid}'", LogEventLevel.Debug),
				new("Sending GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new("Sent GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new($"Sending ID Crypt invitation confirmation to bank '{bankDid}'", LogEventLevel.Debug),
				new($"Sent ID Crypt invitation confirmation to bank '{bankDid}'", LogEventLevel.Debug)
			};

			var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptBankInvitationV1Handler", LogEventLevel.Debug);
			debugLogs.Should().BeEquivalentTo(expectedLogs, options => options.WithStrictOrdering());
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

			receivedMessage.MessageIdentifier.Should().Be("idcrypt.invitationconfirmation.v1");
			receivedMessage.CorrelationId.Should().NotBeNullOrEmpty();

			var receiveInvitationRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler
				.Requests[ReceiveInvitation.Path].Single().RequestUri!.Query);

			var expectedMessageData = new IdCryptInvitationConfirmationV1
			{
				Alias = receiveInvitationRequestQueryParams["alias"],
				AgentPublicDid = GetPublicDid.Response.Result.DID
			};

			var actualMessageData = JsonSerializer.Deserialize<IdCryptInvitationConfirmationV1>(receivedMessage.Data);

			actualMessageData.Should().BeEquivalentTo(expectedMessageData);
		}
	}

	public class WhenGetConnectionPollingExceedsMaxPollTime : IDisposable, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(31_000);
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

		public WhenGetConnectionPollingExceedsMaxPollTime(GrpcServerFixture grpcServer)
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
			_idCryptMessageHandler = StatusCodeHttpHandlerBuilder
				.Create()
				.WithOkResponse(ReceiveInvitation.HttpRequestResponseContext)
				.WithOkResponse(AcceptInvitation.HttpRequestResponseContext)
				.WithOkResponse(GetConnection.HttpRequestResponseContextWithState("request"))
				.WithOkResponse(GetConnection.HttpRequestResponseContextWithState("request"))
				.WithOkResponse(GetConnection.HttpRequestResponseContextWithState("response"))
				.WithOkResponse(GetConnection.HttpRequestResponseContextWithState("response"))
				.WithOkResponse(GetConnection.HttpRequestResponseContextWithState("response"))
				.WithOkResponse(GetPublicDid.HttpRequestResponseContext)
				.Build();

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
		public async Task ThenExceptionIsThrownAndNoMessageSent()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsSubscriber.StartAsync(_allTestHandlers);

			await _fromRtgsSender.SendAsync("idcrypt.invitation.tobank.v1", ValidMessages.IdCryptBankInvitationV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

			_bankInvitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

			var bankDid = ValidMessages.IdCryptBankInvitationV1.FromBankDid;
			var connectionId = ReceiveInvitation.Response.ConnectionID;

			var expectedLogs = new List<LogEntry>
			{
				new($"Sending ReceiveAcceptInvitation request to ID Crypt for invitation from bank '{bankDid}'", LogEventLevel.Debug),
				new($"Sent ReceiveAcceptInvitation request to ID Crypt for invitation from bank '{bankDid}'", LogEventLevel.Debug),
				new($"Polling for connection '{connectionId}' state for invitation from bank '{bankDid}'", LogEventLevel.Debug),
				new($"Sending GetConnection request to ID Crypt with connection Id '{connectionId}'", LogEventLevel.Debug),
				new($"Sent GetConnection request to ID Crypt with connection Id '{connectionId}'", LogEventLevel.Debug),
				new($"Sending GetConnection request to ID Crypt with connection Id '{connectionId}'", LogEventLevel.Debug),
				new($"Sent GetConnection request to ID Crypt with connection Id '{connectionId}'", LogEventLevel.Debug),
				new($"Sending GetConnection request to ID Crypt with connection Id '{connectionId}'", LogEventLevel.Debug),
				new($"Sent GetConnection request to ID Crypt with connection Id '{connectionId}'", LogEventLevel.Debug),
				new($"Sending GetConnection request to ID Crypt with connection Id '{connectionId}'", LogEventLevel.Debug),
				new($"Sent GetConnection request to ID Crypt with connection Id '{connectionId}'", LogEventLevel.Debug)
			};

			var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptBankInvitationV1Handler", LogEventLevel.Debug);
			debugLogs.Should().BeEquivalentTo(expectedLogs, options => options.WithStrictOrdering());

			var errorLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptBankInvitationV1Handler", LogEventLevel.Error);
			errorLogs.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
				$"Error occured when polling for connection '{connectionId}' state for invitation from bank '{bankDid}'",
				LogEventLevel.Error,
				typeof(RtgsSubscriberException)));

			_idCryptMessageHandler.Requests.Should().NotContainKey(GetPublicDid.Path);

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();
			receiver.Connections.Should().BeEmpty();

			_bankInvitationNotificationHandler.ReceivedMessage.Should().BeNull();
		}
	}
}