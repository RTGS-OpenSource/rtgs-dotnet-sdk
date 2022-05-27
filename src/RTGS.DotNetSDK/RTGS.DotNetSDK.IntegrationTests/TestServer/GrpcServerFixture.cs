namespace RTGS.DotNetSDK.IntegrationTests.TestServer;

public class GrpcServerFixture : GrpcServerFixture<TestPaymentService> { }

public class GrpcServerFixture<TPaymentService> : IAsyncLifetime where TPaymentService : class
{
	private readonly GrpcTestServer<TPaymentService> _server;

	public GrpcServerFixture()
	{
		_server = new GrpcTestServer<TPaymentService>();
	}

	public Uri ServerUri { get; private set; }

	public IServiceProvider Services => _server.Services;

	public async Task InitializeAsync()
	{
		try
		{
			ServerUri = await _server.StartAsync();
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
		_server?.Dispose();

		return Task.CompletedTask;
	}

	public void Reset()
	{
		var toRtgsReceiver = _server.Services.GetRequiredService<ToRtgsReceiver>();
		toRtgsReceiver.Connections.Clear();
		toRtgsReceiver.ThrowOnConnection = false;

		var toRtgsMessageHandler = _server.Services.GetRequiredService<ToRtgsMessageHandler>();
		toRtgsMessageHandler.Clear();

		var fromRtgsSender = _server.Services.GetRequiredService<FromRtgsSender>();
		fromRtgsSender.Reset();
	}
}
