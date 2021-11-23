using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.DotNetSDK.Publisher.Extensions;
using RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;
using RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer;
using RTGS.DotNetSDK.Publisher.Messages;
using Xunit;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class GivenMultipleOpenConnections : IAsyncLifetime, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(0.5);

		private readonly GrpcServerFixture _grpcServer;
		private ToRtgsMessageHandler _toRtgsMessageHandler;
		private IHost _clientHost;

		public GivenMultipleOpenConnections(GrpcServerFixture grpcServer)
		{
			_grpcServer = grpcServer;
		}

		public async Task InitializeAsync()
		{
			try
			{
				var rtgsClientOptions = RtgsClientOptions.Builder.CreateNew()
					.BankDid(ValidRequests.BankDid)
					.RemoteHost(_grpcServer.ServerUri.ToString())
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureServices((_, services) => services.AddRtgsPublisher(rtgsClientOptions))
					.Build();

				_toRtgsMessageHandler = _grpcServer.Services.GetRequiredService<ToRtgsMessageHandler>();
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
			_clientHost?.Dispose();

			_grpcServer.Reset();

			return Task.CompletedTask;
		}

		[Fact]
		public void WhenSendingInParallel_ThenCanSendToRtgs()
		{
			const int PublisherCount = 5;

			using var sendRequestsSignal = new ManualResetEventSlim();

			var sendRequestTasks = Enumerable.Range(1, PublisherCount)
				.Select(request => Task.Run(async () =>
				{
					_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

					var rtgsPublisher = _clientHost.Services.GetRequiredService<IRtgsPublisher>();

					sendRequestsSignal.Wait();

					await rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequest());
				})).ToArray();

			sendRequestsSignal.Set();

			var allCompleted = Task.WaitAll(sendRequestTasks, TimeSpan.FromSeconds(5));
			allCompleted.Should().BeTrue();

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();
			receiver.Connections.Count.Should().Be(PublisherCount);
		}
	}
}
