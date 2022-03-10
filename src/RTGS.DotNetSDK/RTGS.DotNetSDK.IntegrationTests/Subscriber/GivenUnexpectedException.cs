using RTGS.DotNetSDK.Subscriber;
using RTGS.DotNetSDK.Subscriber.HandleMessageCommands;
using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber;

public class GivenUnexpectedException : IAsyncDisposable
{
	private static readonly TimeSpan WaitForExceptionDuration = TimeSpan.FromSeconds(30);

	private readonly OutOfMemoryException _thrownException;
	private readonly ITestCorrelatorContext _serilogContext;
	private readonly IHost _clientHost;
	private readonly IRtgsSubscriber _rtgsSubscriber;

	public GivenUnexpectedException()
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

		_thrownException = new OutOfMemoryException("test");

		_clientHost = Host.CreateDefaultBuilder()
			.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
			.ConfigureServices(services =>
			{
				services.AddRtgsSubscriber(rtgsSdkOptions);

				services.AddTransient<IHandleMessageCommandsFactory>(_ => new ThrowHandleMessageCommandsFactory(_thrownException));
			})
			.UseSerilog()
			.Build();

		_rtgsSubscriber = _clientHost.Services.GetRequiredService<IRtgsSubscriber>();
	}

	private class ThrowHandleMessageCommandsFactory : IHandleMessageCommandsFactory
	{
		private readonly Exception _exception;

		public ThrowHandleMessageCommandsFactory(Exception exception)
		{
			_exception = exception;
		}

		public IEnumerable<IHandleMessageCommand> CreateAll(IReadOnlyCollection<IHandler> handlers) =>
			throw _exception;
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
	public async Task WhenFatalErrorIsRaised_ThenIsRunningShouldBeFalse()
	{
		using var raisedExceptionSignal = new ManualResetEventSlim();

		_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedExceptionSignal.Set();

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		raisedExceptionSignal.Wait(WaitForExceptionDuration);

		_rtgsSubscriber.IsRunning.Should().Be(false);
	}

	[Fact]
	public async Task WhenStarting_ThenFatalExceptionEventIsRaised()
	{
		using var raisedExceptionSignal = new ManualResetEventSlim();
		ExceptionEventArgs raisedArgs = null;

		_rtgsSubscriber.OnExceptionOccurred += (_, args) =>
		{
			raisedArgs = args;
			raisedExceptionSignal.Set();
		};

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		raisedExceptionSignal.Wait(WaitForExceptionDuration);

		using var _ = new AssertionScope();

		raisedArgs.Should().NotBeNull();
		raisedArgs?.Exception.Should().BeSameAs(_thrownException);
		raisedArgs?.IsFatal.Should().BeTrue();
	}

	[Fact]
	public async Task WhenStarting_ThenExceptionIsLogged()
	{
		using var raisedExceptionSignal = new ManualResetEventSlim();

		_rtgsSubscriber.OnExceptionOccurred += (_, _) => raisedExceptionSignal.Set();

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		raisedExceptionSignal.Wait(WaitForExceptionDuration);

		var errorLogs = _serilogContext.SubscriberLogs(LogEventLevel.Error);
		errorLogs.Should().BeEquivalentTo(new[] { new LogEntry("An unknown error occurred", LogEventLevel.Error, _thrownException.GetType()) });
	}
}
