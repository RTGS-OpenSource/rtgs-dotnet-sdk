extern alias RTGSServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace RTGS.DotNetSDK.IntegrationTests.TestServer;

public sealed class GrpcTestServer<TPaymentService> : IDisposable where TPaymentService : class
{
	private const int Port = 5100;

	private IHost _host;
	private bool _disposedValue;

	public IServiceProvider Services => _host.Services;

	public async Task<Uri> StartAsync()
	{
		_host = CreateHost();

		await _host.StartAsync();

		return new Uri($"http://localhost:{Port}");
	}

	public async Task StopAsync() =>
		await _host.StopAsync();

	private static IHost CreateHost()
	{
		var builder = new HostBuilder()
			.ConfigureWebHostDefaults(webHost =>
				webHost
					.UseStartup<TestServerStartup>()
					.UseKestrel(kestrelServerOptions => kestrelServerOptions.ListenLocalhost(Port,
						listenOptions => listenOptions.Protocols = HttpProtocols.Http2)));

		return builder.Build();
	}

	private void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				_host?.Dispose();
			}

			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private class TestServerStartup
	{
		public static void ConfigureServices(IServiceCollection services)
		{
			services.AddGrpc();
			services.AddSingleton<ToRtgsReceiver>();
			services.AddSingleton<ToRtgsMessageHandler>();
			services.AddSingleton<FromRtgsSender>();
		}

		public static void Configure(IApplicationBuilder app)
		{
			app.UseRouting();
			app.UseEndpoints(endpoints => endpoints.MapGrpcService<TPaymentService>());
		}
	}
}
