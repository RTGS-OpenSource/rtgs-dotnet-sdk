using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestServer
{
	public class GrpcServerFixture : IAsyncLifetime
	{
		private readonly GrpcTestServer _server;

		public GrpcServerFixture()
		{
			_server = new GrpcTestServer();
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
			var fromRtgsSender = _server.Services.GetRequiredService<FromRtgsSender>();
			fromRtgsSender.Clear();
		}
	}
}
