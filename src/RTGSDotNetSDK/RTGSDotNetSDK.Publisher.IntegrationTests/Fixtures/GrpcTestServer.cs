using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.InMemory;
using Xunit;
using Xunit.Abstractions;

namespace RTGSDotNetSDK.Publisher.IntegrationTests.Fixtures
{
	public class GivenOpenConnection : IDisposable
	{
		public GivenOpenConnection(ITestOutputHelper outputHelper)
		{
			_server = new GrpcTestServer(outputHelper);
			
			var httpClient = _server.Start();

			var rtgsClientOptions = RtgsClientOptions.Builder.CreateNew()
				.BankDid("test-bank-did")
				.RemoteHost("https://localhost:5001")
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureServices((context, services) => 
					services.AddRtgsPublisher(rtgsClientOptions, grpcClientBuilder => grpcClientBuilder.))
				.Build();

			// _clientHost = new object();
			// _clientHost.AddRtgsPublisher(null, grpcClient => grpcClient.ConfigureHttpClient = httpClient);
		}

		public void Dispose()
		{
			_server?.Dispose();
		}

		[Fact]
		public async Task ThenCanSendAtomicLockRequestToRtgs()
		{
			var atomicLockRequest = new AtomicLockRequest();
		}
	}

	public sealed class GrpcTestServer : IDisposable
	{
		private IHost _host;
		private TestServer _testServer;
		private bool _disposedValue;

		public IServiceProvider Services => _testServer.Services;

		public HttpClient Start(ITestOutputHelper outputHelper)
		{
			_host = CreateHost(outputHelper);
			_testServer = _host.GetTestServer();

			return new HttpClient(_testServer.CreateHandler())
			{
				BaseAddress = new Uri("http://localhost:5100")
			};
		}

		private static IHost CreateHost(ITestOutputHelper outputHelper)
		{
			var loggerFactory = new LoggerFactory();
			loggerFactory.AddProvider(new XUnitLoggerProvider(outputHelper));

			var builder = new HostBuilder()
				.ConfigureWebHostDefaults(webHost => webHost.UseTestServer())
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

	internal class XUnitLoggerProvider : ILoggerProvider
	{
		private readonly ITestOutputHelper _outputHelper;

		public XUnitLoggerProvider(ITestOutputHelper outputHelper)
		{
			_outputHelper = outputHelper;
		}

		public ILogger CreateLogger(string categoryName) => new XUnitLogger(_outputHelper, categoryName);

		public void Dispose() => Expression.Empty();
	}
}
