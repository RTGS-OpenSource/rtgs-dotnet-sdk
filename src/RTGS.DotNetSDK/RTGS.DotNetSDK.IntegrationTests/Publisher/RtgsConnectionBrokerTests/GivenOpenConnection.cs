using System.Net.Http;
using System.Text.Json;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;
using RTGS.DotNetSDK.Publisher.Exceptions;
using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.RtgsConnectionBrokerTests;

public class GivenOpenConnection
{
	public sealed class AndIdCryptApiAvailable : IDisposable, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);
		private static readonly Uri IdCryptServiceAddress = new("https://id-crypt-service");

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;

		private StatusCodeHttpHandler _idCryptServiceHttpHandler;
		private IHost _clientHost;
		private IRtgsConnectionBroker _rtgsConnectionBroker;
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
			try
			{
				var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
						TestData.ValidMessages.RtgsGlobalId,
						_grpcServer.ServerUri,
						IdCryptServiceAddress)
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.KeepAlivePingDelay(TimeSpan.FromSeconds(30))
					.KeepAlivePingTimeout(TimeSpan.FromSeconds(30))
					.Build();

				_idCryptServiceHttpHandler = StatusCodeHttpHandlerBuilderFactory
					.Create()
					.WithOkResponse(CreateConnection.HttpRequestResponseContext)
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services
						.AddRtgsPublisher(rtgsSdkOptions)
						.AddTestIdCryptServiceHttpClient(_idCryptServiceHttpHandler))
					.UseSerilog()
				.Build();

				_rtgsConnectionBroker = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
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
		public async Task WhenCallingIdCryptService_ThenUriIsExpected()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsConnectionBroker.SendInvitationAsync();

			var actualApiUri = _idCryptServiceHttpHandler.Requests[CreateConnection.Path]
				.Single()
				.RequestUri
				!.GetLeftPart(UriPartial.Authority);

			actualApiUri.Should().BeEquivalentTo(IdCryptServiceAddress.GetLeftPart(UriPartial.Authority));
		}

		[Fact]
		public async Task WhenCallingIdCryptService_ThenConnectionBrokerLogs()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsConnectionBroker.SendInvitationAsync();

			var alias = CreateConnection.Response.Alias;

			var expectedDebugLogs = new List<LogEntry>
			{
				new($"Sending Id Crypt Invitation with alias {alias} to RTGS", LogEventLevel.Debug),
				new($"Sending Id Crypt Invitation with alias {alias} to RTGS", LogEventLevel.Debug)
			};

			var debugLogs = _serilogContext.ConnectionBrokerLogs(LogEventLevel.Debug);
			debugLogs.Should().BeEquivalentTo(expectedDebugLogs, options => options.WithStrictOrdering());
		}

		[Fact]
		public async Task WhenCallingIdCryptService_ThenIdCryptServiceClientLogs()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsConnectionBroker.SendInvitationAsync();

			var expectedDebugLogs = new List<LogEntry>
			{
				new("Sending CreateConnectionInvitation request to ID Crypt Service", LogEventLevel.Debug),
				new("Sent CreateConnectionInvitation request to ID Crypt Service", LogEventLevel.Debug)
			};

			var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.IdCrypt.IdCryptServiceClient", LogEventLevel.Debug);
			debugLogs.Should().BeEquivalentTo(expectedDebugLogs, options => options.WithStrictOrdering());
		}

		[Fact]
		public async Task WhenCallingIdCryptService_ThenInvitationIsCorrectlyMapped()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsConnectionBroker.SendInvitationAsync();

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();
			var receivedMessage = receiver.Connections
				.Should().ContainSingle().Which.Requests
				.Should().ContainSingle().Subject;

			using var _ = new AssertionScope();

			receivedMessage.MessageIdentifier.Should().Be("idcrypt.invitation.tortgs.v1");
			receivedMessage.CorrelationId.Should().NotBeNullOrEmpty();

			var response = CreateConnection.Response;

			var expectedMessageData = new IdCryptInvitationV1
			{
				Alias = response.Alias,
				Id = response.Invitation.Id,
				Label = response.Invitation.Label,
				RecipientKeys = response.Invitation.RecipientKeys,
				ServiceEndpoint = response.Invitation.ServiceEndpoint,
				Type = response.Invitation.Type,
				AgentPublicDid = response.AgentPublicDid
			};

			var actualMessageData = JsonSerializer
				.Deserialize<IdCryptInvitationV1>(receivedMessage.Data.Span);

			actualMessageData.Should().BeEquivalentTo(expectedMessageData);
		}
	}

	public sealed class AndIdCryptServiceCreateConnectionApiUnavailable : IDisposable, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;

		private StatusCodeHttpHandler _idCryptServiceHttpHandler;
		private IHost _clientHost;
		private IRtgsConnectionBroker _rtgsConnectionBroker;
		private ToRtgsMessageHandler _toRtgsMessageHandler;

		public AndIdCryptServiceCreateConnectionApiUnavailable(GrpcServerFixture grpcServer)
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
						TestData.ValidMessages.RtgsGlobalId,
						_grpcServer.ServerUri,
						new Uri("https://id-crypt-service"))
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.KeepAlivePingDelay(TimeSpan.FromSeconds(30))
					.KeepAlivePingTimeout(TimeSpan.FromSeconds(30))
					.Build();

				_idCryptServiceHttpHandler = StatusCodeHttpHandlerBuilderFactory
					.Create()
					.WithServiceUnavailableResponse(CreateConnection.Path)
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services
						.AddRtgsPublisher(rtgsSdkOptions)
						.AddTestIdCryptServiceHttpClient(_idCryptServiceHttpHandler))
					.UseSerilog()
					.Build();

				_rtgsConnectionBroker = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
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
		public async Task ThenExceptionIsThrown()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await FluentActions.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync())
				.Should()
				.ThrowAsync<RtgsPublisherException>()
				.WithMessage("Error occurred creating ID Crypt invitation")
				.WithInnerException(typeof(HttpRequestException));
		}

		[Fact]
		public async Task ThenMessageNotSent()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await FluentActions.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync())
			  .Should()
			  .ThrowAsync<Exception>();

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			receiver.Connections.Should().BeEmpty();
		}

		[Fact]
		public async Task ThenRtgsConnectionBrokerLogs()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await FluentActions.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync())
				.Should()
				.ThrowAsync<Exception>();

			var errorLogs = _serilogContext.ConnectionBrokerLogs(LogEventLevel.Error);
			errorLogs.Should().ContainSingle().Which.Should()
				.BeEquivalentTo(new LogEntry(
					"Error occurred creating ID Crypt invitation",
					LogEventLevel.Error,
					typeof(RtgsPublisherException)));
		}

		[Fact]
		public async Task ThenIdCryptServiceClientLogs()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await FluentActions.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync())
				.Should()
				.ThrowAsync<Exception>();

			using var _ = new AssertionScope();

			var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.IdCrypt.IdCryptServiceClient", LogEventLevel.Debug);
			debugLogs.Should().ContainSingle().Which
				.Should().BeEquivalentTo(new LogEntry("Sending CreateConnectionInvitation request to ID Crypt Service", LogEventLevel.Debug));

			var errorLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.IdCrypt.IdCryptServiceClient", LogEventLevel.Error);
			errorLogs.Should().ContainSingle().Which
				.Should().BeEquivalentTo(new LogEntry(
					"Error occurred when sending CreateConnectionInvitation request to ID Crypt Service",
					LogEventLevel.Error,
					typeof(HttpRequestException)));
		}
	}

	public sealed class AndShortTestWaitForAcknowledgementDuration : IDisposable, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;

		private QueueableStatusCodeHttpHandler _idCryptServiceHttpHandler;
		private IHost _clientHost;
		private IRtgsConnectionBroker _rtgsConnectionBroker;
		private ToRtgsMessageHandler _toRtgsMessageHandler;

		public AndShortTestWaitForAcknowledgementDuration(GrpcServerFixture grpcServer)
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
						TestData.ValidMessages.RtgsGlobalId,
						_grpcServer.ServerUri,
						new Uri("https://id-crypt-service"))
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.KeepAlivePingDelay(TimeSpan.FromSeconds(30))
					.KeepAlivePingTimeout(TimeSpan.FromSeconds(30))
					.Build();

				_idCryptServiceHttpHandler = StatusCodeHttpHandlerBuilderFactory
					.CreateQueueable()
					.WithOkResponse(CreateConnection.HttpRequestResponseContext)
					.WithOkResponse(CreateConnection.HttpRequestResponseContext)
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services
						.AddRtgsPublisher(rtgsSdkOptions)
						.AddTestIdCryptServiceHttpClient(_idCryptServiceHttpHandler))
					.UseSerilog()
					.Build();

				_rtgsConnectionBroker = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
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
		public async Task WhenSendingMessageAndSuccessAcknowledgementReceived_ThenLogInformation()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsConnectionBroker.SendInvitationAsync();

			var expectedInformationLogs = new List<LogEntry>
			{
				new("Sending IdCryptInvitationV1 to RTGS (SendIdCryptInvitationToRtgsAsync)", LogEventLevel.Information),
				new("Sent IdCryptInvitationV1 to RTGS (SendIdCryptInvitationToRtgsAsync)", LogEventLevel.Information),
				new("Received IdCryptInvitationV1 acknowledgement (acknowledged) from RTGS (SendIdCryptInvitationToRtgsAsync)", LogEventLevel.Information)
			};

			using var _ = new AssertionScope();

			var informationLogs = _serilogContext.PublisherLogs(LogEventLevel.Information);
			informationLogs.Should().BeEquivalentTo(expectedInformationLogs, options => options.WithStrictOrdering());

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

			var expectedInformationLogs = new List<LogEntry>
			{
				new("Sending IdCryptInvitationV1 to RTGS (SendIdCryptInvitationToRtgsAsync)", LogEventLevel.Information),
				new("Sent IdCryptInvitationV1 to RTGS (SendIdCryptInvitationToRtgsAsync)", LogEventLevel.Information),

			};

			using var _ = new AssertionScope();

			var informationLogs = _serilogContext.PublisherLogs(LogEventLevel.Information);
			informationLogs.Should().BeEquivalentTo(expectedInformationLogs, options => options.WithStrictOrdering());

			var warningLogs = _serilogContext.PublisherLogs(LogEventLevel.Warning);
			warningLogs.Should().BeEmpty();

			var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
			errorLogs.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
				"Received IdCryptInvitationV1 acknowledgement (rejected) from RTGS (SendIdCryptInvitationToRtgsAsync)",
				LogEventLevel.Error));
		}

		[Fact]
		public async Task WhenSendingMessageAndRpcExceptionReceived_ThenLog()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ThrowRpcException(StatusCode.Unavailable, "test"));

			await FluentActions.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync())
				.Should()
				.ThrowAsync<RpcException>();

			var expectedInformationLogs = new List<LogEntry>
			{
				new("Sending IdCryptInvitationV1 to RTGS (SendIdCryptInvitationToRtgsAsync)", LogEventLevel.Information),
				new("Sent IdCryptInvitationV1 to RTGS (SendIdCryptInvitationToRtgsAsync)", LogEventLevel.Information)
			};

			using var _ = new AssertionScope();

			var informationLogs = _serilogContext.PublisherLogs(LogEventLevel.Information);
			informationLogs.Should().BeEquivalentTo(expectedInformationLogs, options => options.WithStrictOrdering());

			var warningLogs = _serilogContext.PublisherLogs(LogEventLevel.Warning);
			warningLogs.Should().BeEmpty();

			var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
			errorLogs.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
				"Error received when sending IdCryptInvitationV1 to RTGS (SendIdCryptInvitationToRtgsAsync)",
				LogEventLevel.Error,
				typeof(RpcException)));
		}

		[Fact]
		public async Task ThenSeeRtgsGlobalIdInRequestHeader()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsConnectionBroker.SendInvitationAsync();

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			var connection = receiver.Connections.SingleOrDefault();

			connection.Should().NotBeNull();
			connection!.Headers.Should()
				.ContainSingle(header => header.Key == "rtgs-global-id"
										 && header.Value == TestData.ValidMessages.RtgsGlobalId);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgement_ThenReturnSuccess()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			result.Should().Be(SendResult.Success);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsUnsuccessfulAcknowledgement_ThenReturnRejected()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithFailure());

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			result.Should().Be(SendResult.Rejected);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgementTooLate_ThenReturnTimeout()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1))));

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			result.Should().Be(SendResult.Timeout);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgementTooLate_ThenLogError()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1))));

			await _rtgsConnectionBroker.SendInvitationAsync();

			var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
			errorLogs.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
				"Timed out waiting for IdCryptInvitationV1 acknowledgement from RTGS (SendIdCryptInvitationToRtgsAsync)",
				LogEventLevel.Error));
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

			result1.Should().Be(SendResult.Success);

			result2.Should().Be(SendResult.Timeout);
		}

		[Fact]
		public async Task WhenBankMessageApiOnlyReturnsUnexpectedAcknowledgement_ThenReturnTimeout()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnUnexpectedAcknowledgementWithSuccess());

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			result.Should().Be(SendResult.Timeout);
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

			result.Should().Be(SendResult.Rejected);
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

			result.Should().Be(SendResult.Rejected);
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

			result.Should().Be(SendResult.Success);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsSuccessForSecondMessageOnly_ThenDoNotTimeout()
		{
			var result1 = await _rtgsConnectionBroker.SendInvitationAsync();
			result1.Should().Be(SendResult.Timeout);

			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			var result2 = await _rtgsConnectionBroker.SendInvitationAsync();
			result2.Should().Be(SendResult.Success);
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
			result.Should().Be(SendResult.Success);
		}
	}

	public sealed class AndLongTestWaitForAcknowledgementDuration : IDisposable, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(30);
		private static readonly TimeSpan TestWaitForSendDuration = TimeSpan.FromSeconds(15);

		private readonly GrpcServerFixture _grpcServer;

		private QueueableStatusCodeHttpHandler _idCryptServiceHttpHandler;
		private IHost _clientHost;
		private IRtgsConnectionBroker _rtgsConnectionBroker;

		public AndLongTestWaitForAcknowledgementDuration(GrpcServerFixture grpcServer)
		{
			_grpcServer = grpcServer;

			SetupDependencies();
		}

		private void SetupDependencies()
		{
			try
			{
				var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
						TestData.ValidMessages.RtgsGlobalId,
						_grpcServer.ServerUri,
						new Uri("https://id-crypt-service"))
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.Build();

				_idCryptServiceHttpHandler = StatusCodeHttpHandlerBuilderFactory
					.CreateQueueable()
					.WithOkResponse(CreateConnection.HttpRequestResponseContext)
					.WithOkResponse(CreateConnection.HttpRequestResponseContext)
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services
						.AddRtgsPublisher(rtgsSdkOptions)
						.AddTestIdCryptServiceHttpClient(_idCryptServiceHttpHandler))
					.Build();

				_rtgsConnectionBroker = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
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
		public async Task WhenCancellationTokenIsCancelledBeforeAcknowledgementTimeout_ThenThrowOperationCancelled()
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
}
