﻿using System;
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
	public class GivenInitialFailedConnection : IAsyncLifetime, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

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
				var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(ValidMessages.BankDid, _grpcServer.ServerUri)
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services.AddRtgsPublisher(rtgsPublisherOptions))
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

		[Fact]
		public async Task WhenSending_ThenThrowException()
		{
			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			receiver.ThrowOnConnection = true;

			await FluentActions
				.Awaiting(() => _rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequestV1()))
				.Should()
				.ThrowAsync<Exception>();
		}

		[Fact]
		public async Task WhenSendingBigMessage_ThenThrowException()
		{
			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			receiver.ThrowOnConnection = true;

			await FluentActions
				.Awaiting(() => _rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequestV1 { EndToEndId = new string('e', 100_000) }))
				.Should()
				.ThrowAsync<Exception>();
		}

		[Fact]
		public async Task WhenSubsequentConnectionCanBeOpened_ThenCanSendSubsequentMessagesToRtgs()
		{
			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			receiver.ThrowOnConnection = true;

			await FluentActions
				.Awaiting(() => _rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequestV1()))
				.Should()
				.ThrowAsync<Exception>();

			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			receiver.ThrowOnConnection = false;

			var result = await _rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequestV1());

			result.Should().Be(SendResult.Success);
		}
	}
}
