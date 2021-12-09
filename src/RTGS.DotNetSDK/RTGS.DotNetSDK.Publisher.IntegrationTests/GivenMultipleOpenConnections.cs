using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
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
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

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
				var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(ValidMessages.BankDid, _grpcServer.ServerUri)
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services.AddRtgsPublisher(rtgsPublisherOptions))
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

					await using var rtgsPublisher = _clientHost.Services.GetRequiredService<IRtgsPublisher>();

					sendRequestsSignal.Wait();

					await rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequest());
				})).ToArray();

			sendRequestsSignal.Set();

			var allCompleted = Task.WaitAll(sendRequestTasks, TimeSpan.FromSeconds(5));
			allCompleted.Should().BeTrue();

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();
			receiver.Connections.Count.Should().Be(PublisherCount);
		}

		[Fact]
		public async Task WhenSendingSequentially_ThenCanSendToRtgs()
		{
			const int PublisherCount = 5;

			await using var rtgsPublisher1 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
			await using var rtgsPublisher2 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
			await using var rtgsPublisher3 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
			await using var rtgsPublisher4 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
			await using var rtgsPublisher5 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();

			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
			await rtgsPublisher1.SendAtomicLockRequestAsync(new AtomicLockRequest());

			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
			await rtgsPublisher2.SendAtomicLockRequestAsync(new AtomicLockRequest());

			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
			await rtgsPublisher3.SendAtomicLockRequestAsync(new AtomicLockRequest());

			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
			await rtgsPublisher4.SendAtomicLockRequestAsync(new AtomicLockRequest());

			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
			await rtgsPublisher5.SendAtomicLockRequestAsync(new AtomicLockRequest());

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			using var _ = new AssertionScope();
			receiver.Connections.Count.Should().Be(PublisherCount);
			receiver.Connections.SelectMany(connection => connection.Requests).Count().Should().Be(5);
		}
	}
}
