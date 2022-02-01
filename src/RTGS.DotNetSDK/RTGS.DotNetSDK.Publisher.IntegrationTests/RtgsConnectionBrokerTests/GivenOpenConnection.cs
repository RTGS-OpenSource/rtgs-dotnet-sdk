using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using IDCryptGlobal.Cloud.Agent.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RTGS.DotNetSDK.Publisher.IntegrationTests.RtgsConnectionBrokerTests.HttpHandlers;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.RtgsConnectionBrokerTests;

public class GivenOpenConnection
{
	public class AndShortTestWaitForAcknowledgementDuration : IAsyncLifetime, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;

		private IRtgsConnectionBroker _rtgsConnectionBroker;
		private ToRtgsMessageHandler _toRtgsMessageHandler;
		private IHost _clientHost;


		public AndShortTestWaitForAcknowledgementDuration(GrpcServerFixture grpcServer)
		{
			_grpcServer = grpcServer;

			SetupSerilogLogger();

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

		public async Task InitializeAsync()
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

				var responseJson = "{\"connection_id\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," +
							   "\"invitation\":{" +
							   "\"@ID\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," +
							   "\"@Type\":\"https://didcomm.org/my-family/1.0/my-message-type\"," +
							   "\"label\":\"Bob\"," +
							   "\"recipientKeys\":[" +
							   "\"H3C2AVvLMv6gmMNam3uVAjZpfkcJCwDwnZn6z3wXmqPV\"]," +
							   "\"serviceEndpoint\":\"http://192.168.56.101:8020\"}}";

				var statusCodeHttpHandler = new StatusCodeHttpHandler(HttpStatusCode.OK, new StringContent(responseJson));

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services
						.AddRtgsPublisher(rtgsPublisherOptions)
						.AddSingleton(statusCodeHttpHandler)
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
				await DisposeAsync();

				throw;
			}
		}

		public async Task DisposeAsync()
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

            var receivedRequest = JsonConvert.DeserializeObject<IdCryptInvitationV1>(receivedMessage.Data);
            receivedRequest.Should().BeEquivalentTo(publisherAction.Request, options => options.ComparingByMembers<TRequest>());
        }

		// TODO: RBEN - Deferred for later
        //[Fact]
        //public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgement_ThenReturnSuccess()
        //{
        //    _toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

        //    var connectionId = await _rtgsConnectionBroker.SendInvitationAsync();

        //    connectionId.Should().Be("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        //}

	}
}
