extern alias RTGSServer;
using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Publisher.IntegrationTests.Logging;
using Xunit.Abstractions;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public sealed class GrpcTestServer : IDisposable
	{
		private readonly ITestOutputHelper _outputHelper;
		private IHost _host;
		private TestServer _testServer;
		private bool _disposedValue;

		public GrpcTestServer(ITestOutputHelper outputHelper)
		{
			_outputHelper = outputHelper;
		}

		public IServiceProvider Services => _testServer.Services;

		public HttpClient Start()
		{
			_host = CreateHost(_outputHelper);
			_testServer = _host.GetTestServer();

			return new HttpClient(_testServer.CreateHandler())
			{
				BaseAddress = new Uri("http://localhost")
			};
		}

		private static IHost CreateHost(ITestOutputHelper outputHelper)
		{
			var loggerFactory = new LoggerFactory();
			loggerFactory.AddProvider(new XUnitLoggerProvider(outputHelper));

			var builder = new HostBuilder()
				.ConfigureWebHostDefaults(webHost => webHost.UseTestServer().UseStartup<TestServerStartup>())
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
					_testServer?.Dispose();
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
	}

	public class TestServerStartup
	{
		public IConfiguration Configuration { get; set; }

		public void ConfigureServices(IServiceCollection services) =>
			services.AddGrpc();

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseRouting();
			app.UseEndpoints(endpoints => endpoints.MapGrpcService<TestPaymentService>());
		}
	}
}
