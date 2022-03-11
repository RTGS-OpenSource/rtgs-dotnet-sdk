using System.Net;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using RTGS.DotNetSDK.Publisher.IntegrationTests.Extensions;
using RTGS.DotNetSDK.Publisher.IntegrationTests.HttpHandlers;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.RtgsConnectionBrokerTests;

public class GivenOpenConnection
{
	public class AndIdCryptApiAvailable : IDisposable, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);
		private const string IdCryptApiKey = "id-crypt-api-key";
		private static readonly Uri IdCryptApiUri = new("http://id-crypt-cloud-agent-api.com");

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;

		private IRtgsConnectionBroker _rtgsConnectionBroker;
		private ToRtgsMessageHandler _toRtgsMessageHandler;
		private IHost _clientHost;
		private StatusCodeHttpHandler _idCryptMessageHandler;

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
				var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(
						ValidMessages.BankDid,
						_grpcServer.ServerUri,
						IdCryptApiUri,
						IdCryptApiKey,
						new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.KeepAlivePingDelay(TimeSpan.FromSeconds(30))
					.KeepAlivePingTimeout(TimeSpan.FromSeconds(30))
					.Build();

				_idCryptMessageHandler = new StatusCodeHttpHandler(IdCryptEndPoints.MockHttpResponses);

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services
						.AddRtgsPublisher(rtgsPublisherOptions)
						.AddTestIdCryptHttpClient(_idCryptMessageHandler))
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

		[Theory]
		[InlineData(IdCryptEndPoints.PublicDidPath)]
		[InlineData(IdCryptEndPoints.InvitationPath)]
		public async Task WhenCallingIdCryptAgent_ThenApiKeyHeaderIsExpected(string path)
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsConnectionBroker.SendInvitationAsync();

			var actualApiKey = _idCryptMessageHandler
				.Requests[path]
				.Headers
				.GetValues("X-API-Key")
				.Single();

			actualApiKey.Should().Be(IdCryptApiKey);
		}

		[Theory]
		[InlineData(IdCryptEndPoints.PublicDidPath)]
		[InlineData(IdCryptEndPoints.InvitationPath)]
		public async Task WhenCallingIdCryptAgent_ThenUriIsExpected(string path)
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsConnectionBroker.SendInvitationAsync();

			var actualApiUri = _idCryptMessageHandler
				.Requests[path]
				.RequestUri
				.GetLeftPart(UriPartial.Authority);

			actualApiUri.Should().BeEquivalentTo(IdCryptApiUri.GetLeftPart(UriPartial.Authority));
		}

		[Fact]
		public async Task WhenCallingIdCryptAgent_ThenAliasIsAlwaysUnique()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsConnectionBroker.SendInvitationAsync();

			var inviteRequestQueryParams1 = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri.Query);
			var alias1 = inviteRequestQueryParams1["alias"];

			await _rtgsConnectionBroker.SendInvitationAsync();

			var inviteRequestQueryParams2 = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri.Query);
			var alias2 = inviteRequestQueryParams2["alias"];

			alias2.Should().NotBeEquivalentTo(alias1);
		}

		[Fact]
		public async Task WhenCallingIdCryptAgent_ThenDefaultQueryParamsAreCorrect()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsConnectionBroker.SendInvitationAsync();

			var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri.Query);
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
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsConnectionBroker.SendInvitationAsync();

			var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri.Query);
			var alias = inviteRequestQueryParams["alias"];

			var expectedLogs = new List<LogEntry>
			{
				new($"Sending CreateInvitation request with alias {alias} to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new("Sent CreateInvitation request to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new("Sending GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new("Sent GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug)
			};

			var debugLogs = _serilogContext.ConnectionBrokerLogs(LogEventLevel.Debug);
			debugLogs.Should().BeEquivalentTo(expectedLogs, options => options.WithStrictOrdering());
		}

		[Fact]
		public async Task ThenCanSendRequestToRtgsAndInvitationIsCorrectlyMapped()
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

			var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri.Query);

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

	public class AndIdCryptCreateInvitationApiUnavailable : IDisposable, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;

		private IRtgsConnectionBroker _rtgsConnectionBroker;
		private ToRtgsMessageHandler _toRtgsMessageHandler;
		private IHost _clientHost;
		private StatusCodeHttpHandler _idCryptMessageHandler;

		public AndIdCryptCreateInvitationApiUnavailable(GrpcServerFixture grpcServer)
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
				var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(
						ValidMessages.BankDid,
						_grpcServer.ServerUri,
						new Uri("http://id-crypt-cloud-agent-api.com"),
						"id-crypt-api-key",
						new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.KeepAlivePingDelay(TimeSpan.FromSeconds(30))
					.KeepAlivePingTimeout(TimeSpan.FromSeconds(30))
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
					.ConfigureServices(services => services
						.AddRtgsPublisher(rtgsPublisherOptions)
						.AddTestIdCryptHttpClient(_idCryptMessageHandler))
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
			  .ThrowAsync<Exception>();
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
		public async Task ThenLog()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await FluentActions.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync())
			  .Should()
			  .ThrowAsync<Exception>();

			var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri.Query);
			var alias = inviteRequestQueryParams["alias"];

			using var _ = new AssertionScope();
			var debugLogs = _serilogContext.ConnectionBrokerLogs(LogEventLevel.Debug);
			debugLogs.Select(log => log.Message)
				.Should().ContainSingle(msg => msg == $"Sending CreateInvitation request with alias {alias} to ID Crypt Cloud Agent");

			var errorLogs = _serilogContext.ConnectionBrokerLogs(LogEventLevel.Error);
			errorLogs.Select(log => log.Message)
				.Should().ContainSingle(msg => msg == "Error occurred when sending CreateInvitation request to ID Crypt Cloud Agent");
		}
	}

	public class AndIdCryptGetPublicDIDApiUnavailable : IDisposable, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;

		private IRtgsConnectionBroker _rtgsConnectionBroker;
		private ToRtgsMessageHandler _toRtgsMessageHandler;
		private IHost _clientHost;
		private StatusCodeHttpHandler _idCryptMessageHandler;

		public AndIdCryptGetPublicDIDApiUnavailable(GrpcServerFixture grpcServer)
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
				var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(
						ValidMessages.BankDid,
						_grpcServer.ServerUri,
						new Uri("http://id-crypt-cloud-agent-api.com"),
						"id-crypt-api-key",
						new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.KeepAlivePingDelay(TimeSpan.FromSeconds(30))
					.KeepAlivePingTimeout(TimeSpan.FromSeconds(30))
					.Build();

				var mockHttpResponses = new List<MockHttpResponse> {
					new MockHttpResponse {
						Content = null ,
						HttpStatusCode = HttpStatusCode.ServiceUnavailable,
						Path = IdCryptEndPoints.PublicDidPath
					},
					new MockHttpResponse {
						Content =new StringContent ( IdCryptTestMessages.ConnectionInviteResponseJson ),
						HttpStatusCode = HttpStatusCode.OK,
						Path = IdCryptEndPoints.InvitationPath
					}
				};

				_idCryptMessageHandler = new StatusCodeHttpHandler(mockHttpResponses);

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services
						.AddRtgsPublisher(rtgsPublisherOptions)
						.AddTestIdCryptHttpClient(_idCryptMessageHandler))
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
			  .ThrowAsync<Exception>();
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
		public async Task ThenLog()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await FluentActions.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync())
			  .Should()
			  .ThrowAsync<Exception>();

			var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri.Query);
			var alias = inviteRequestQueryParams["alias"];

			var expectedDebugLogs = new List<LogEntry>
			{
				new ($"Sending CreateInvitation request with alias {alias} to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new ("Sent CreateInvitation request to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new ("Sending GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug),
				new ("Sent GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug)
			};

			var expectedErrorLogMessage = "Error occurred when sending GetPublicDid request to ID Crypt Cloud Agent";

			using var _ = new AssertionScope();
			var debugLogs = _serilogContext.ConnectionBrokerLogs(LogEventLevel.Debug);
			debugLogs.Should().BeEquivalentTo(expectedDebugLogs);

			var errorLogs = _serilogContext.ConnectionBrokerLogs(LogEventLevel.Error).SingleOrDefault();
			errorLogs.Message.Should().Be(expectedErrorLogMessage);
		}
	}

	public class AndShortTestWaitForAcknowledgementDuration : IDisposable, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;

		private IRtgsConnectionBroker _rtgsConnectionBroker;
		private ToRtgsMessageHandler _toRtgsMessageHandler;
		private StatusCodeHttpHandler _idCryptMessageHandler;
		private IHost _clientHost;

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
				var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(
						ValidMessages.BankDid,
						_grpcServer.ServerUri,
						new Uri("http://id-crypt-cloud-agent-api.com"),
						"id-crypt-api-key",
						new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.KeepAlivePingDelay(TimeSpan.FromSeconds(30))
					.KeepAlivePingTimeout(TimeSpan.FromSeconds(30))
					.Build();

				_idCryptMessageHandler = new StatusCodeHttpHandler(IdCryptEndPoints.MockHttpResponses);

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services
						.AddRtgsPublisher(rtgsPublisherOptions)
						.AddTestIdCryptHttpClient(_idCryptMessageHandler))
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

			var expectedLogs = new List<LogEntry>
			{
				new("Sending IdCryptInvitationV1 to RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Information),
				new("Sent IdCryptInvitationV1 to RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Information),
				new("Received IdCryptInvitationV1 acknowledgement (rejected) from RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Error)
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
		public async Task ThenSeeBankDidInRequestHeader()
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
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgement_ThenReturnSuccessAndConnectionIdAndAlias()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri.Query);
			var alias = inviteRequestQueryParams["alias"];

			using var _ = new AssertionScope();

			result.Alias.Should().Be(alias);
			result.ConnectionId.Should().Be(IdCryptTestMessages.ConnectionInviteResponse.ConnectionID);
			result.SendResult.Should().Be(SendResult.Success);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsUnsuccessfulAcknowledgement_ThenReturnRejectedAndConnectionIdAndAliasAreNull()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithFailure());

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			using var _ = new AssertionScope();

			result.Alias.Should().BeNull();
			result.ConnectionId.Should().BeNull();
			result.SendResult.Should().Be(SendResult.Rejected);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgementTooLate_ThenReturnTimeoutAndConnectionIdAndAliasAreNull()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1))));

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			result.Alias.Should().BeNull();
			result.ConnectionId.Should().BeNull();
			result.SendResult.Should().Be(SendResult.Timeout);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgementTooLate_ThenLogError()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1))));

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
			errorLogs.Select(log => log.Message).Should()
				.ContainSingle("Timed out waiting for IdCryptInvitationV1 acknowledgement from RTGS (SendIdCryptInvitationAsync)");
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

			var inviteRequestQueryParams1 = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri.Query);
			var alias1 = inviteRequestQueryParams1["alias"];

			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1))));
			var result2 = await _rtgsConnectionBroker.SendInvitationAsync();

			using var _ = new AssertionScope();

			result1.Alias.Should().Be(alias1);
			result1.ConnectionId.Should().Be(IdCryptTestMessages.ConnectionInviteResponse.ConnectionID);
			result1.SendResult.Should().Be(SendResult.Success);

			result2.Alias.Should().BeNull();
			result2.ConnectionId.Should().BeNull();
			result2.SendResult.Should().Be(SendResult.Timeout);
		}

		[Fact]
		public async Task WhenBankMessageApiOnlyReturnsUnexpectedAcknowledgement_ThenReturnTimeoutAndConnectionIdAndAliasAreNull()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnUnexpectedAcknowledgementWithSuccess());

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			result.Alias.Should().BeNull();
			result.ConnectionId.Should().BeNull();
			result.SendResult.Should().Be(SendResult.Timeout);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsUnexpectedAcknowledgementBeforeFailureAcknowledgement_ThenReturnRejectedAndConnectionIdAndAliasAreNull()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
			{
				handler.ReturnUnexpectedAcknowledgementWithSuccess();
				handler.ReturnExpectedAcknowledgementWithFailure();
			});

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			result.Alias.Should().BeNull();
			result.ConnectionId.Should().BeNull();
			result.SendResult.Should().Be(SendResult.Rejected);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsFailureAcknowledgementBeforeUnexpectedAcknowledgement_ThenReturnRejectedAndConnectionIdAndAliasAreNull()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
			{
				handler.ReturnExpectedAcknowledgementWithFailure();
				handler.ReturnUnexpectedAcknowledgementWithSuccess();
			});

			var result = await _rtgsConnectionBroker.SendInvitationAsync();

			result.Alias.Should().BeNull();
			result.ConnectionId.Should().BeNull();
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

		private IRtgsConnectionBroker _rtgsConnectionBroker;
		private IHost _clientHost;

		public AndLongTestWaitForAcknowledgementDuration(GrpcServerFixture grpcServer)
		{
			_grpcServer = grpcServer;

			SetupDependencies();
		}

		private void SetupDependencies()
		{
			try
			{

				var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(
						ValidMessages.BankDid,
						_grpcServer.ServerUri,
						new Uri("http://id-crypt-cloud-agent-api.com"),
						"id-crypt-api-key",
						new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.Build();

				var idCryptMessageHandler = new StatusCodeHttpHandler(IdCryptEndPoints.MockHttpResponses);

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services
						.AddRtgsPublisher(rtgsPublisherOptions)
						.AddTestIdCryptHttpClient(idCryptMessageHandler))
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

	private record IdCryptInvitationV1
	{
		public string Alias { get; init; }
		public string Label { get; init; }
		public IEnumerable<string> RecipientKeys { get; init; }
		public string Id { get; init; }
		public string Type { get; init; }
		public string ServiceEndPoint { get; init; }
		public string AgentPublicDid { get; set; }
	}
}
