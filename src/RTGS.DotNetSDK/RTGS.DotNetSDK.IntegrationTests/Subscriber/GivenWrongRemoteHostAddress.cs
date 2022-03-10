using RTGS.DotNetSDK.Subscriber;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber;

public class GivenWrongRemoteHostAddress : IAsyncDisposable
{
	private static readonly TimeSpan WaitForExceptionEventDuration = TimeSpan.FromSeconds(30);

	private readonly ITestCorrelatorContext _serilogContext;
	private readonly IHost _clientHost;
	private readonly IRtgsSubscriber _rtgsSubscriber;

	public GivenWrongRemoteHostAddress()
	{
		SetupSerilogLogger();

		_serilogContext = TestCorrelator.CreateContext();

		var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
				TestData.ValidMessages.BankDid, 
				new Uri("https://localhost:4567"),
				new Uri("http://id-crypt-cloud-agent-api.com"),
				"id-crypt-api-key",
				new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
			.Build();

		_clientHost = Host.CreateDefaultBuilder()
			.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
			.ConfigureServices(services => services.AddRtgsSubscriber(rtgsSdkOptions))
			.UseSerilog()
			.Build();

		_rtgsSubscriber = _clientHost.Services.GetRequiredService<IRtgsSubscriber>();
	}

	private static void SetupSerilogLogger() =>
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Debug()
			.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
			.Enrich.FromLogContext()
			.WriteTo.Console()
			.WriteTo.TestCorrelator()
			.CreateLogger();

	public async ValueTask DisposeAsync()
	{
		await _rtgsSubscriber.DisposeAsync();
		await _clientHost.StopAsync();
	}

	[Fact]
	public async Task WhenStarting_ThenExceptionEventIsRaised()
	{
		using var raisedExceptionSignal = new ManualResetEventSlim();
		ExceptionEventArgs raisedArgs = null;

		_rtgsSubscriber.OnExceptionOccurred += (_, args) =>
		{
			raisedArgs = args;
			raisedExceptionSignal.Set();
		};

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		raisedExceptionSignal.Wait(WaitForExceptionEventDuration);

		using var _ = new AssertionScope();

		raisedArgs.Should().NotBeNull();
		raisedArgs?.Exception.Should().NotBeNull();
		raisedArgs?.IsFatal.Should().BeTrue();
	}

	[Fact]
	public async Task WhenStarting_ThenExceptionIsLogged()
	{
		using var raisedExceptionSignal = new ManualResetEventSlim();

		_rtgsSubscriber.OnExceptionOccurred += (_, _) => raisedExceptionSignal.Set();

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		raisedExceptionSignal.Wait(WaitForExceptionEventDuration);

		var errorLogs = _serilogContext.SubscriberLogs(LogEventLevel.Error);
		errorLogs.Should().BeEquivalentTo(new[] { new LogEntry("An error occurred while communicating with RTGS", LogEventLevel.Error, typeof(RpcException)) });
	}
}
