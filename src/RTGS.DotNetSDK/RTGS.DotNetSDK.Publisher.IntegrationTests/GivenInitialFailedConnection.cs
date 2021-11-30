using System;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.DotNetSDK.Publisher.Extensions;
using RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;
using RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer;
using RTGS.DotNetSDK.Publisher.Messages;
using Xunit;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class GivenInitialFailedConnection : IAsyncLifetime, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(0.5);

		private readonly GrpcServerFixture _grpcServer;

		private IRtgsPublisher _rtgsPublisher;
		private ToRtgsMessageHandler _toRtgsMessageHandler;
		private IHost _clientHost;

		public GivenInitialFailedConnection(GrpcServerFixture grpcServer)
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
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices((_, services) => services.AddRtgsPublisher(rtgsClientOptions))
					.Build();

				_rtgsPublisher = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
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

		public async Task DisposeAsync()
		{
			if (_rtgsPublisher is not null)
			{
				await _rtgsPublisher.DisposeAsync();
			}

			_clientHost?.Dispose();

			_grpcServer.Reset();
		}

		// TODO: Tom
		[Fact]
		public async Task WhenSending_ThenThrowRpcException()
		{
			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			receiver.ThrowOnConnection = true;

			await FluentActions
				.Awaiting(() => _rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequest ()))
				.Should()
				.ThrowAsync<RpcException>();
		}

		[Fact]
		public async Task WhenSendingBigMessage_ThenThrowRpcException()
		{
			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			receiver.ThrowOnConnection = true;

			await FluentActions
				.Awaiting(() => _rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequest { EndToEndId = new string('e', 100_000) }))
				.Should()
				.ThrowAsync<RpcException>();
		}

		//GivenInitialFailedConnectionAndFailedFirstMessage
		//WhenSubsequentConnectionCanBeOpened
		//ThenCanSendSubsequentMessagesToRtgs		
	}
}
