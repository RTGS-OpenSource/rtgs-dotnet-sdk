using RTGS.DotNetSDK.Subscriber;
using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber;

public class GivenUnstableOpenConnection : IAsyncLifetime
{
	private static readonly TimeSpan WaitForExceptionDuration = TimeSpan.FromSeconds(30);

	private readonly GrpcTestServer _server;
	private readonly IEnumerable<IHandler> _testHandlers;
	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;

	public GivenUnstableOpenConnection()
	{
		_server = new GrpcTestServer();
		_testHandlers = new AllTestHandlers().ToList();
	}

	public async Task InitializeAsync()
	{
		try
		{
			var serverUri = await _server.StartAsync();

			var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
					TestData.ValidMessages.BankDid,
					serverUri,
					new Uri("http://id-crypt-cloud-agent-api.com"),
					"id-crypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices((_, services) => services.AddRtgsSubscriber(rtgsSdkOptions))
				.UseSerilog()
				.Build();

			_fromRtgsSender = _server.Services.GetRequiredService<FromRtgsSender>();
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

	public async Task DisposeAsync()
	{
		_clientHost?.Dispose();

		if (_server is not null)
		{
			await _server.StopAsync();
			_server.Dispose();
		}
	}

	[Fact]
	public async Task WhenServerStops_ThenThrowFatalRpcExceptionEvent()
	{
		using var raisedExceptionSignal = new ManualResetEventSlim();
		ExceptionEventArgs raisedArgs = null;

		_rtgsSubscriber.OnExceptionOccurred += (_, args) =>
		{
			raisedExceptionSignal.Set();
			raisedArgs = args;
		};

		await _rtgsSubscriber.StartAsync(_testHandlers);

		await EnsureSubscriberIsRunning();

		await _server.StopAsync();

		raisedExceptionSignal.Wait(WaitForExceptionDuration);

		using var _ = new AssertionScope();

		raisedArgs.Should().NotBeNull();
		raisedArgs?.Exception.Should().BeOfType<RpcException>();
		raisedArgs?.IsFatal.Should().BeTrue();
	}

	[Fact]
	public async Task WhenServerStops_ThenStopSubscriber()
	{
		using var raisedExceptionSignal = new ManualResetEventSlim();

		_rtgsSubscriber.OnExceptionOccurred += (_, _) => raisedExceptionSignal.Set();

		await _rtgsSubscriber.StartAsync(_testHandlers);

		await EnsureSubscriberIsRunning();

		await _server.StopAsync();

		raisedExceptionSignal.Wait(WaitForExceptionDuration);

		_rtgsSubscriber.IsRunning.Should().BeFalse();
	}

	[Fact]
	public async Task WhenServerRestarts_ThenCanRestartSubscriber()
	{
		using var raisedExceptionSignal = new ManualResetEventSlim();

		_rtgsSubscriber.OnExceptionOccurred += (_, _) => raisedExceptionSignal.Set();

		await _rtgsSubscriber.StartAsync(_testHandlers);

		await EnsureSubscriberIsRunning();

		await _server.StopAsync();

		raisedExceptionSignal.Wait(WaitForExceptionDuration);

		await _server.StartAsync();

		await _rtgsSubscriber.StartAsync(_testHandlers);

		await EnsureSubscriberIsRunning();
	}

	private async Task EnsureSubscriberIsRunning()
	{
		await _fromRtgsSender.SendAsync("MessageRejected", TestData.ValidMessages.MessageRejected);

		_rtgsSubscriber.IsRunning.Should().BeTrue();
	}
}
