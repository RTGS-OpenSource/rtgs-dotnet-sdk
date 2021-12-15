using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestServer
{
	public sealed class GrpcTestServer : IDisposable
	{
		private const int Port = 5200;

		private IHost _host;

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

		public void Dispose() =>
			_host?.Dispose();

		private class TestServerStartup
		{
			public static void ConfigureServices(IServiceCollection services)
			{
				services.AddGrpc();
				services.AddSingleton<FromRtgsSender>();
			}

			public static void Configure(IApplicationBuilder app)
			{
				app.UseRouting();
				app.UseEndpoints(endpoints => endpoints.MapGrpcService<TestPaymentService>());
			}
		}
	}
}
