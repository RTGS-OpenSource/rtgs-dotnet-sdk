using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.DotNetSDK.Publisher.Extensions;
using RTGS.DotNetSDK.Publisher.Messages;
using Xunit;
using Xunit.Abstractions;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class GivenOpenConnection : IDisposable
	{
		private readonly GrpcTestServer _server;
		private readonly ITestOutputHelper _outputHelper;
		private readonly IRtgsPublisher _rtgsPublisher;
		private readonly IHost _clientHost;

		public GivenOpenConnection(ITestOutputHelper outputHelper)
		{
			_server = new GrpcTestServer(outputHelper);

			var httpClient = _server.Start();

			var rtgsClientOptions = RtgsClientOptions.Builder.CreateNew()
				.BankDid("test-bank-did")
				.RemoteHost(httpClient.BaseAddress!.ToString())
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureServices((context, services) =>
					services.AddRtgsPublisher(rtgsClientOptions, grpcClientBuilder => grpcClientBuilder.ConfigureChannel(channel =>
					{
						channel.HttpClient = httpClient;
						channel.HttpHandler = null;
					})))
				.Build();

			_outputHelper = outputHelper;
			_rtgsPublisher = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
		}

		public void Dispose()
		{
			_server?.Dispose();
			_clientHost?.Dispose();
		}

		[Fact]
		public async Task ThenCanSendAtomicLockRequestToRtgs()
		{
			var atomicLockRequest = new AtomicLockRequest();
		}
	}
}
