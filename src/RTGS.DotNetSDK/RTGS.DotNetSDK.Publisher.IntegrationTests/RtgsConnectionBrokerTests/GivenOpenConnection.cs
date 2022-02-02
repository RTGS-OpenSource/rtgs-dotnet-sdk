using System.Net;
using System.Net.Http;
using IDCryptGlobal.Cloud.Agent.Identity;
using IDCryptGlobal.Cloud.Agent.Identity.Connection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RTGS.DotNetSDK.Publisher.IntegrationTests.HttpHandlers;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.RtgsConnectionBrokerTests;

public class GivenOpenConnection
{
	public class AndShortTestWaitForAcknowledgementDuration : IDisposable, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;

		private IRtgsConnectionBroker _rtgsConnectionBroker;
		private ToRtgsMessageHandler _toRtgsMessageHandler;
		private IHost _clientHost;
        private StatusCodeHttpHandler _idCryptMessageHandler;
		private readonly ConnectionInviteResponseModel _connectionInviteResponse;

		public AndShortTestWaitForAcknowledgementDuration(GrpcServerFixture grpcServer)
		{
			_grpcServer = grpcServer;

            _connectionInviteResponse = new ConnectionInviteResponseModel
            {
                ConnectionID = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                Invitation = new ConnectionInvitation
                {
                    ID = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                    Type = "https://didcomm.org/my-family/1.0/my-message-type",
                    Label = "Bob",
                    RecipientKeys = new[]
                    {
                        "H3C2AVvLMv6gmMNam3uVAjZpfkcJCwDwnZn6z3wXmqPV"
                    },
                    ServiceEndPoint = "http://192.168.56.101:8020"
                }
            };

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
				var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(
						ValidMessages.BankDid,
						_grpcServer.ServerUri,
						Guid.NewGuid().ToString(),
						new Uri("http://example.com"),
						"http://example.com")
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.KeepAlivePingDelay(TimeSpan.FromSeconds(30))
					.KeepAlivePingTimeout(TimeSpan.FromSeconds(30))
					.Build();

                var connectionInviteResponseJson = JsonConvert.SerializeObject(_connectionInviteResponse);

				_idCryptMessageHandler = new StatusCodeHttpHandler(HttpStatusCode.OK, new StringContent(connectionInviteResponseJson));

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services
						.AddRtgsPublisher(rtgsPublisherOptions)
						.AddSingleton(_idCryptMessageHandler)
						.AddHttpClient<IIdentityClient, IdentityClient>((httpClient, serviceProvider) =>
							{
								var identityOptions = serviceProvider.GetRequiredService<IOptions<IdentityConfig>>();
								var identityClient = new IdentityClient(httpClient, identityOptions);
								
								return identityClient;
							})
						.AddHttpMessageHandler<StatusCodeHttpHandler>())
					.UseSerilog()
					.Build();

				_rtgsConnectionBroker = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
				_toRtgsMessageHandler = _grpcServer.Services.GetRequiredService<ToRtgsMessageHandler>();
			}
			catch (Exception)
			{
				// If an exception occurs then manually clean up as IAsyncLifetime.DisposeAsync is not called.
				// See https://github.com/xunit/xunit/discussions/2313 for further details.
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
		public async Task WhenSendingMessageAndSuccessAcknowledgementReceived_ThenLogInformation()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsConnectionBroker.SendInvitationAsync();

			var expectedLogs = new List<LogEntry>
			{
				new("Sending IdCryptInvitationV1 to RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Information),
				new("Sent IdCryptInvitationV1 to RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Information),
				new("Received IdCryptInvitationV1 acknowledgement (acknowledged) from RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Information)
			};

			using var _ = new AssertionScope();

			var informationLogs = _serilogContext.PublisherLogs(LogEventLevel.Information);
			informationLogs.Should().BeEquivalentTo(expectedLogs, options => options.WithStrictOrdering());

			var warningLogs = _serilogContext.PublisherLogs(LogEventLevel.Warning);
			warningLogs.Should().BeEmpty();

			var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
			errorLogs.Should().BeEmpty();
		}

		[Fact]
		public async Task WhenSendingMessageAndFailedAcknowledgementReceived_ThenLog()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithFailure());

			await _rtgsConnectionBroker.SendInvitationAsync();

			var exepctedLogs = new List<LogEntry>
			{
				new("Sending IdCryptInvitationV1 to RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Information),
				new("Sent IdCryptInvitationV1 to RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Information),
				new("Received IdCryptInvitationV1 acknowledgement (rejected) from RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Error)
			};

			using var _ = new AssertionScope();

			var informationLogs = _serilogContext.PublisherLogs(LogEventLevel.Information);
			informationLogs.Should().BeEquivalentTo(exepctedLogs.Where(log => log.LogLevel is LogEventLevel.Information), options => options.WithStrictOrdering());

			var warningLogs = _serilogContext.PublisherLogs(LogEventLevel.Warning);
			warningLogs.Should().BeEmpty();

			var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
			errorLogs.Should().BeEquivalentTo(exepctedLogs.Where(log => log.LogLevel is LogEventLevel.Error), options => options.WithStrictOrdering());
		}

		[Fact]
		public async Task WhenSendingMessageAndRpcExceptionReceived_ThenLog()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => 
				handler.ThrowRpcException(StatusCode.Unavailable, "test"));

			await FluentActions.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync())
				.Should()
				.ThrowAsync<RpcException>();

			var expectedLogs = new List<LogEntry>
			{
				new("Sending IdCryptInvitationV1 to RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Information),
				new("Sent IdCryptInvitationV1 to RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Information),
				new("Error received when sending IdCryptInvitationV1 to RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Error, typeof(RpcException))
			};

			using var _ = new AssertionScope();

			var informationLogs = _serilogContext.PublisherLogs(LogEventLevel.Information);
			informationLogs.Should().BeEquivalentTo(expectedLogs.Where(log => log.LogLevel is LogEventLevel.Information), options => options.WithStrictOrdering());

			var warningLogs = _serilogContext.PublisherLogs(LogEventLevel.Warning);
			warningLogs.Should().BeEmpty();

			var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
			errorLogs.Should().BeEquivalentTo(expectedLogs.Where(log => log.LogLevel is LogEventLevel.Error), options => options.WithStrictOrdering());
		}
		
		[Fact]
		public async Task WhenUsingMetadata_ThenSeeBankDidInRequestHeader()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsConnectionBroker.SendInvitationAsync();

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			var connection = receiver.Connections.SingleOrDefault();

			connection.Should().NotBeNull();
			connection!.Headers.Should()
                .ContainSingle(header => header.Key == "bankdid" 
                                         && header.Value == ValidMessages.BankDid);
		}

        [Fact]
        public async Task ThenCanSendRequestToRtgs()
        {
            _toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

            await _rtgsConnectionBroker.SendInvitationAsync();

            var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();
            var receivedMessage = receiver.Connections
                .Should().ContainSingle().Which.Requests
                .Should().ContainSingle().Subject;

            using var _ = new AssertionScope();

            receivedMessage.MessageIdentifier.Should().Be("idcrypt.invitation.v1");
            receivedMessage.CorrelationId.Should().NotBeNullOrEmpty();

			var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Request.RequestUri.Query);

			var expectedMessageData = new IdCryptInvitationV1
            {
                Alias = inviteRequestQueryParams["alias"],
                Label = _connectionInviteResponse.Invitation.Label,
                RecipientKeys = _connectionInviteResponse.Invitation.RecipientKeys,
                Id = _connectionInviteResponse.Invitation.ID,
                Type = _connectionInviteResponse.Invitation.Type,
                ServiceEndPoint = _connectionInviteResponse.Invitation.ServiceEndPoint
            };

            var actualMessageData = JsonConvert
                .DeserializeObject<IdCryptInvitationV1>(receivedMessage.Data);

            actualMessageData.Should().BeEquivalentTo(expectedMessageData);
        }

		[Fact]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgement_ThenReturnSuccessAndConnectionId()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			using var _ = new AssertionScope();

			result.ConnectionId.Should().Be(_connectionInviteResponse.ConnectionID);
			result.SendResult.Should().Be(SendResult.Success);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsUnsuccessfulAcknowledgement_ThenReturnRejectedAndConnectionIdIsNull()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithFailure());

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			using var _ = new AssertionScope();

			result.ConnectionId.Should().BeNull();
			result.SendResult.Should().Be(SendResult.Rejected);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgementTooLate_ThenReturnTimeoutAndConnectionIdIsNull()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1))));

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			result.ConnectionId.Should().BeNull();
			result.SendResult.Should().Be(SendResult.Timeout);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgementTooLate_ThenLogError()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1))));

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			var expectedLogs = new List<LogEntry>
			{
				new("Timed out waiting for IdCryptInvitationV1 acknowledgement from RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Error)
			};

			var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
			errorLogs.Should().BeEquivalentTo(expectedLogs, options => options.WithStrictOrdering());
		}

		[Fact]
		public async Task WhenSendingMultipleMessages_ThenOnlyOneConnection()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnUnexpectedAcknowledgementWithSuccess());
			await _rtgsConnectionBroker.SendInvitationAsync();

			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnUnexpectedAcknowledgementWithSuccess());
			await _rtgsConnectionBroker.SendInvitationAsync();

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			receiver.NumberOfConnections.Should().Be(1);
		}

		[Fact]
		public async Task WhenSendingMultipleMessagesAndLastOneTimesOut_ThenDoNotSeePreviousSuccess()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());
			var result1 = await _rtgsConnectionBroker.SendInvitationAsync();

			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1))));
			var result2 = await _rtgsConnectionBroker.SendInvitationAsync();

			using var _ = new AssertionScope();

			result1.SendResult.Should().Be(SendResult.Success);
			result1.ConnectionId.Should().Be(_connectionInviteResponse.ConnectionID);

			result2.SendResult.Should().Be(SendResult.Timeout);
			result2.ConnectionId.Should().BeNull();
		}

		[Fact]
		public async Task WhenBankMessageApiOnlyReturnsUnexpectedAcknowledgement_ThenReturnTimeoutAndConnectionIdIsNull()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnUnexpectedAcknowledgementWithSuccess());

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			result.SendResult.Should().Be(SendResult.Timeout);
			result.ConnectionId.Should().BeNull();
		}

        [Fact]
        public async Task WhenBankMessageApiReturnsUnexpectedAcknowledgementBeforeFailureAcknowledgement_ThenReturnRejected()
        {
            _toRtgsMessageHandler.SetupForMessage(handler =>
            {
                handler.ReturnUnexpectedAcknowledgementWithSuccess();
                handler.ReturnExpectedAcknowledgementWithFailure();
            });

            var result = await _rtgsConnectionBroker.SendInvitationAsync();

            result.SendResult.Should().Be(SendResult.Rejected);
        }

        [Fact]
        public async Task WhenBankMessageApiReturnsFailureAcknowledgementBeforeUnexpectedAcknowledgement_ThenReturnRejected()
        {
            _toRtgsMessageHandler.SetupForMessage(handler =>
            {
                handler.ReturnExpectedAcknowledgementWithFailure();
                handler.ReturnUnexpectedAcknowledgementWithSuccess();
            });

            var result = await _rtgsConnectionBroker.SendInvitationAsync();

            result.SendResult.Should().Be(SendResult.Rejected);
        }

        [Fact]
        public async Task WhenBankMessageApiReturnsSuccessWrappedByUnexpectedFailureAcknowledgements_ThenReturnSuccess()
        {
            _toRtgsMessageHandler.SetupForMessage(handler =>
            {
                handler.ReturnUnexpectedAcknowledgementWithFailure();
                handler.ReturnExpectedAcknowledgementWithSuccess();
                handler.ReturnUnexpectedAcknowledgementWithFailure();
            });

            var result = await _rtgsConnectionBroker.SendInvitationAsync();

            result.SendResult.Should().Be(SendResult.Success);
        }

        [Fact]
        public async Task WhenBankMessageApiReturnsSuccessForSecondMessageOnly_ThenDoNotTimeout()
        {
            var result1 = await _rtgsConnectionBroker.SendInvitationAsync();
            result1.SendResult.Should().Be(SendResult.Timeout);

            _toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

            var result2 = await _rtgsConnectionBroker.SendInvitationAsync();
            result2.SendResult.Should().Be(SendResult.Success);
        }

        [Fact]
        public async Task WhenBankMessageApiThrowsExceptionForFirstMessage_ThenStillHandleSecondMessage()
        {
            _toRtgsMessageHandler.SetupForMessage(handler => handler.ThrowRpcException(StatusCode.Unknown, "test"));

            await FluentActions.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync())
                .Should()
                .ThrowAsync<RpcException>();

            _toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

            var result = await _rtgsConnectionBroker.SendInvitationAsync();
            result.SendResult.Should().Be(SendResult.Success);
        }
    }

    public class AndLongTestWaitForAcknowledgementDuration : IDisposable, IClassFixture<GrpcServerFixture>
    {
        private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan TestWaitForSendDuration = TimeSpan.FromSeconds(15);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;

		private IRtgsConnectionBroker _rtgsConnectionBroker;
		private ToRtgsMessageHandler _toRtgsMessageHandler;
		private IHost _clientHost;
		private StatusCodeHttpHandler _idCryptMessageHandler;
		private ConnectionInviteResponseModel _connectionInviteResponse;

		public AndLongTestWaitForAcknowledgementDuration(GrpcServerFixture grpcServer)
		{
			_grpcServer = grpcServer;

            _connectionInviteResponse = new ConnectionInviteResponseModel
            {
                ConnectionID = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                Invitation = new ConnectionInvitation
                {
                    ID = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                    Type = "https://didcomm.org/my-family/1.0/my-message-type",
                    Label = "Bob",
                    RecipientKeys = new[]
                    {
                        "H3C2AVvLMv6gmMNam3uVAjZpfkcJCwDwnZn6z3wXmqPV"
                    },
                    ServiceEndPoint = "http://192.168.56.101:8020"
                }
            };

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
				var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(
						ValidMessages.BankDid,
						_grpcServer.ServerUri,
						Guid.NewGuid().ToString(),
						new Uri("http://example.com"),
						"http://example.com")
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.KeepAlivePingDelay(TimeSpan.FromSeconds(30))
					.KeepAlivePingTimeout(TimeSpan.FromSeconds(30))
					.Build();

				var connectionInviteResponseJson = JsonConvert.SerializeObject(_connectionInviteResponse);

				_idCryptMessageHandler = new StatusCodeHttpHandler(HttpStatusCode.OK, new StringContent(connectionInviteResponseJson));

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services
						.AddRtgsPublisher(rtgsPublisherOptions)
						.AddSingleton(_idCryptMessageHandler)
						.AddHttpClient<IIdentityClient, IdentityClient>((httpClient, serviceProvider) =>
						{
							var identityOptions = serviceProvider.GetRequiredService<IOptions<IdentityConfig>>();
							var identityClient = new IdentityClient(httpClient, identityOptions);

							return identityClient;
						})
						.AddHttpMessageHandler<StatusCodeHttpHandler>())
					.UseSerilog()
					.Build();

				_rtgsConnectionBroker = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
				_toRtgsMessageHandler = _grpcServer.Services.GetRequiredService<ToRtgsMessageHandler>();
			}
			catch (Exception)
			{
				// If an exception occurs then manually clean up as IAsyncLifetime.DisposeAsync is not called.
				// See https://github.com/xunit/xunit/discussions/2313 for further details.
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
        public async Task WhenCancellationTokenIsCancelledBeforeAcknowledgmentTimeout_ThenThrowOperationCancelled()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TestWaitForSendDuration);

            var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();
            receiver.RegisterOnMessageReceived(() => cancellationTokenSource.Cancel());

            await FluentActions.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync(cancellationTokenSource.Token))
                .Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task WhenCancellationTokenIsCancelledBeforeSemaphoreIsEntered_ThenThrowOperationCancelled()
        {
            var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

            using var firstMessageReceivedSignal = new ManualResetEventSlim();
            receiver.RegisterOnMessageReceived(() => firstMessageReceivedSignal.Set());

            // Send the first message that has no acknowledgement setup so the client
            // will hold on to the semaphore for a long time.
            using var firstMessageCancellationTokenSource = new CancellationTokenSource(TestWaitForSendDuration);
            var firstMessageTask = FluentActions
                .Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync(firstMessageCancellationTokenSource.Token))
                .Should()
                .ThrowAsync<OperationCanceledException>();

            // Once the server has received the first message we know the semaphore is in use...
            firstMessageReceivedSignal.Wait(TestWaitForSendDuration);

            // ...we can send the second message knowing it will be waiting due to the semaphore.
            using var secondMessageCancellationTokenSource = new CancellationTokenSource(TestWaitForSendDuration);
            var secondMessageTask = FluentActions
                .Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync(secondMessageCancellationTokenSource.Token))
                .Should()
                .ThrowAsync<OperationCanceledException>();

            // While the first message's acknowledgement is still being waited, cancel the second message before it is sent.
            secondMessageCancellationTokenSource.Cancel();
            await secondMessageTask;

            // Allow the test to gracefully stop.
            firstMessageCancellationTokenSource.Cancel();
            await firstMessageTask;

            receiver.Connections.Single().Requests.Count().Should().Be(1, "the second message should not have been sent as the semaphore should not be entered");
        }
	}

    private record IdCryptInvitationV1
    {
        public string Alias { get; init; }
        public string Label { get; init; }
        public IEnumerable<string> RecipientKeys { get; init; }
        public string Id { get; init; }
        public string Type { get; init; }
        public string ServiceEndPoint { get; init; }
    }
}
