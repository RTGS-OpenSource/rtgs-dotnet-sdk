namespace RTGS.DotNetSDK.Subscriber.IntegrationTests;

public class GivenServerStops : IAsyncLifetime, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(100);
	private static readonly TimeSpan WaitForAcknowledgementsDuration = TimeSpan.FromMilliseconds(100);
	private static readonly TimeSpan WaitForExceptionEventDuration = TimeSpan.FromMilliseconds(100);

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;
	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;

	public GivenServerStops(GrpcServerFixture grpcServer)
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
			var rtgsSubscriberOptions = RtgsSubscriberOptions.Builder.CreateNew(ValidMessages.BankDid, _grpcServer.ServerUri)
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices((_, services) => services.AddRtgsSubscriber(rtgsSubscriberOptions))
				.UseSerilog()
				.Build();

			_fromRtgsSender = _grpcServer.Services.GetRequiredService<FromRtgsSender>();
			_rtgsSubscriber = _clientHost.Services.GetRequiredService<IRtgsSubscriber>();
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

		_grpcServer.Reset();

		return Task.CompletedTask;
	}

	[Fact]
	public async Task ThenThrowFatalRpcExceptionEvent()
	{
		using var raisedExceptionSignal = new ManualResetEventSlim();
		ExceptionEventArgs raisedArgs = null;

		_rtgsSubscriber.OnExceptionOccurred += (_, args) =>
		{
			raisedExceptionSignal.Set();
			raisedArgs = args;
		};

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		await _grpcServer.StopAsync();

		var waitForExceptionDuration = TimeSpan.FromSeconds(30);
		raisedExceptionSignal.Wait(waitForExceptionDuration);

		raisedArgs?.Exception.Should().BeOfType<RpcException>();
		raisedArgs?.IsFatal.Should().BeTrue();
	}

	[Fact]
	public async Task ThenStopSubscriber()
	{
		using var raisedExceptionSignal = new ManualResetEventSlim();

		_rtgsSubscriber.OnExceptionOccurred += (_, args) =>	raisedExceptionSignal.Set();

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		await _grpcServer.StopAsync();

		var waitForExceptionDuration = TimeSpan.FromSeconds(30);
		raisedExceptionSignal.Wait(waitForExceptionDuration);

		_rtgsSubscriber.IsRunning.Should().Be(false);
	}
}
