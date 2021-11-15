extern alias RTGSServer;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer
{
	public sealed class GrpcTestServer : IDisposable
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

		private IHost CreateHost()
		{
			var builder = new HostBuilder()
				.ConfigureWebHostDefaults(webHost =>
					webHost
						.UseStartup<TestServerStartup>()
						.UseKestrel(kestrelServerOptions => kestrelServerOptions.Listen(IPAddress.Any, Port,
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
			public void ConfigureServices(IServiceCollection services)
			{
				services.AddGrpc();
				services.AddSingleton<ToRtgsReceiver>();
				services.AddSingleton<ToRtgsMessageHandler>();
			}

			public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
			{
				app.UseRouting();
				app.UseEndpoints(endpoints => endpoints.MapGrpcService<TestPaymentService>());
			}
		}
	}
}
