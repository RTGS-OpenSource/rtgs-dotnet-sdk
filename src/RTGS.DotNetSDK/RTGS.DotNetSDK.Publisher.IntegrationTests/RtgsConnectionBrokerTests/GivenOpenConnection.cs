using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using IDCryptGlobal.Cloud.Agent.Identity;
using Microsoft.Extensions.Http;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.RtgsConnectionBrokerTests;

public class GivenOpenConnection
{
	public class AndShortTestWaitForAcknowledgementDuration : DelegatingHandler, IAsyncLifetime, IClassFixture<GrpcServerFixture>
	{
		private const string BankPartnerDid = "bank-partner-did";
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;

		private IRtgsConnectionBroker _rtgsRtgsConnectionBroker;
		private ToRtgsMessageHandler _toRtgsMessageHandler;
		private IHost _clientHost;

		private string _actualRequest;
		private string _actualRequestAbsoluteUri;


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
						"",
						"",
						"")
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.KeepAlivePingDelay(TimeSpan.FromSeconds(30))
					.KeepAlivePingTimeout(TimeSpan.FromSeconds(30))
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services
						.AddRtgsPublisher(rtgsPublisherOptions)
						.ConfigureAll<HttpClientFactoryOptions>(
							options => options.HttpMessageHandlerBuilderActions.Add(
								handlerBuilder => handlerBuilder.AdditionalHandlers.Add(this)))
						)
					.UseSerilog()
					.Build();

				_rtgsRtgsConnectionBroker = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
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

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			_actualRequestAbsoluteUri = request.RequestUri!.AbsoluteUri;

			_actualRequest = await request.Content!.ReadAsStringAsync(cancellationToken);

			return new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("expected-response", Encoding.UTF8, MediaTypeNames.Text.Plain)
			};
		}

		[Fact]
		public async Task WhenSendingMessageAndSuccessAcknowledgementReceived_ThenLogInformation()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsRtgsConnectionBroker.SendInvitationAsync();

			using var _ = new AssertionScope();

			var expectedInformationLogs = new List<LogEntry>
			{
				new("Sending IdCryptInvitationV1 to RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Information),
				new("Sent IdCryptInvitationV1 to RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Information),
				new("Received IdCryptInvitationV1 acknowledgement (acknowledged) from RTGS (SendIdCryptInvitationAsync)", LogEventLevel.Information)
			};

			var informationLogs = _serilogContext.PublisherLogs(LogEventLevel.Information);
			informationLogs.Should().BeEquivalentTo(expectedInformationLogs, options => options.WithStrictOrdering());

			var warningLogs = _serilogContext.PublisherLogs(LogEventLevel.Warning);
			warningLogs.Should().BeEmpty();

			var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
			errorLogs.Should().BeEmpty();
		}

	}
}
