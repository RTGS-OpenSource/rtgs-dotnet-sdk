extern alias RTGSServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Publisher.IntegrationTests.Logging;
using System;
using System.Net;
using Xunit.Abstractions;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer
{
	public sealed class GrpcTestServer : IDisposable
	{
		private const int Port = 5100;

		private readonly ITestOutputHelper _outputHelper;
		private IHost _host;
		private bool _disposedValue;

		public GrpcTestServer(ITestOutputHelper outputHelper)
		{
			_outputHelper = outputHelper;
		}

		public IServiceProvider Services => _host.Services;

		public Uri Start()
		{
			_host = CreateHost();

			return new Uri($"http://localhost:{Port}");
		}

		private IHost CreateHost()
		{
			var loggerFactory = new LoggerFactory();
			loggerFactory.AddProvider(new XUnitLoggerProvider(_outputHelper));

			var builder = new HostBuilder()
				.ConfigureWebHostDefaults(webHost =>
					webHost
						.UseStartup<TestServerStartup>()
						.UseKestrel(kestrelServerOptions => kestrelServerOptions.Listen(IPAddress.Any, Port,
							listenOptions => listenOptions.Protocols = HttpProtocols.Http2)))
				.ConfigureServices(services => services.AddSingleton<ILoggerFactory>(loggerFactory));

			return builder.Start();
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
