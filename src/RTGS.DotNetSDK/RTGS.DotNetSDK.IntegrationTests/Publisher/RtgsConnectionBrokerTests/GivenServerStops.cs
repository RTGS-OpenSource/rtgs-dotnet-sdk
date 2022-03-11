using RTGS.DotNetSDK.IntegrationTests.Publisher.Extensions;
using RTGS.DotNetSDK.IntegrationTests.Publisher.HttpHandlers;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.RtgsConnectionBrokerTests;

public class GivenServerStops : IAsyncLifetime
{
	private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

	private readonly GrpcTestServer _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;
	private ToRtgsMessageHandler _toRtgsMessageHandler;
	private IHost _clientHost;
	private IRtgsConnectionBroker _rtgsConnectionBroker;

	public GivenServerStops()
	{
		_grpcServer = new GrpcTestServer();

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
			var serverUri = await _grpcServer.StartAsync();

			var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
					TestData.ValidMessages.BankDid,
					serverUri,
					new Uri("http://id-crypt-cloud-agent-api.com"),
					"id-crypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
				.Build();


			var idCryptMessageHandler = new StatusCodeHttpHandler(IdCryptEndPoints.MockHttpResponses);

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices(services => services
					.AddRtgsPublisher(rtgsSdkOptions)
					.AddTestIdCryptHttpClient(idCryptMessageHandler))
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

	public Task DisposeAsync()
	{
		_clientHost?.Dispose();

		_grpcServer.Dispose();

		return Task.CompletedTask;
	}

	[Fact]
	public async Task WhenSendingMessage_ThenRpcExceptionOrIOExceptionThrown()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		var result = await _rtgsConnectionBroker.SendInvitationAsync();
		result.SendResult.Should().Be(SendResult.Success);

		await _grpcServer.StopAsync();

		var exceptionAssertions = await FluentActions.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync())
			.Should().ThrowAsync<Exception>();

		// One of two exceptions can be thrown depending on how far along the call is.
		exceptionAssertions.And.GetType()
			.Should().Match(exceptionType => exceptionType == typeof(RpcException)
											 || exceptionType == typeof(IOException));
	}

	[Fact]
	public async Task WhenNotSendingMessage_ThenLogError()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsConnectionBroker.SendInvitationAsync();

		await _grpcServer.StopAsync();

		_clientHost.Dispose();

		var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
		errorLogs.Should().BeEquivalentTo(new[]
		{
			new LogEntry("RTGS connection unexpectedly closed", LogEventLevel.Error, typeof(RpcException))
		});
	}
}
