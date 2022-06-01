extern alias RTGSServer;
using RTGS.DotNetSDK.Subscriber;
using RTGSServer::RTGS.Public.Payment.V4;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber;

public sealed class GivenFromResponseStreamCompleted : IClassFixture<GrpcServerFixture<GivenFromResponseStreamCompleted.CompletingPaymentService>>, IDisposable
{
	private static readonly TimeSpan WaitForExceptionDuration = TimeSpan.FromSeconds(5);
	private readonly ITestCorrelatorContext _serilogContext;
	private readonly GrpcServerFixture<CompletingPaymentService> _grpcServer;
	private IHost _clientHost;
	private IRtgsSubscriber _rtgsSubscriber;

	public GivenFromResponseStreamCompleted(GrpcServerFixture<CompletingPaymentService> grpcServer)
	{
		_grpcServer = grpcServer;

		SetupSerilogLogger();

		SetupDependencies();

		_serilogContext = TestCorrelator.CreateContext();
	}

	private void SetupDependencies()
	{
		try
		{
			var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
					TestData.ValidMessages.RtgsGlobalId,
					_grpcServer.ServerUri,
					new Uri("https://id-crypt-service"))
				.EnableMessageSigning()
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices((_, services) => services
					.AddRtgsSubscriber(rtgsSdkOptions))
				.UseSerilog()
				.Build();

			_rtgsSubscriber = _clientHost.Services.GetRequiredService<IRtgsSubscriber>();
		}
		catch (Exception)
		{
			Dispose();

			throw;
		}
	}

	private static void SetupSerilogLogger() =>
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Debug()
			.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
			.Enrich.FromLogContext()
			.WriteTo.Console()
			.WriteTo.TestCorrelator()
			.CreateLogger();


	public void Dispose()
	{
		_clientHost?.Dispose();
		_serilogContext?.Dispose();
	}

	[Fact]
	public async Task WhenUnexpected_ThenThrowFatalRpcExceptionEvent()
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

		raisedArgs.Should().NotBeNull();

		using var _ = new AssertionScope();

		raisedArgs.Exception.Should().BeOfType<RtgsSubscriberException>();
		raisedArgs.IsFatal.Should().BeTrue();
	}

	[Fact]
	public async Task WhenUnexpected_ThenLogError()
	{
		using var raisedExceptionSignal = new ManualResetEventSlim();

		_rtgsSubscriber.OnExceptionOccurred += (_, _) =>
		{
			raisedExceptionSignal.Set();
		};

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		raisedExceptionSignal.Wait(WaitForExceptionDuration);

		var errorLogs = _serilogContext.SubscriberLogs(LogEventLevel.Error);
		errorLogs.Should().BeEquivalentTo(new[]
		{
			new LogEntry("The call completed although stop was not requested",
				LogEventLevel.Error, typeof(RtgsSubscriberException))
		});
	}

	public class CompletingPaymentService : Payment.PaymentBase
	{
		public override Task ToRtgsMessage(IAsyncStreamReader<RtgsMessage> requestStream,
			IServerStreamWriter<RtgsMessageAcknowledgement> responseStream, ServerCallContext context) =>
			Task.CompletedTask;

		public override Task FromRtgsMessage(IAsyncStreamReader<RtgsMessageAcknowledgement> requestStream,
			IServerStreamWriter<RtgsMessage> responseStream, ServerCallContext context) => Task.CompletedTask;
	}
}
