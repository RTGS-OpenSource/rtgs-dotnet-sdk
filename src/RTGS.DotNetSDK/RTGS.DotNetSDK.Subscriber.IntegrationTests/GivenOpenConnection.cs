using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.DotNetSDK.Subscriber.Extensions;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestServer;
using RTGS.Public.Payment.V2;
using Xunit;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests
{
	public class GivenOpenConnection : IAsyncLifetime, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(500);

		private readonly GrpcServerFixture _grpcServer;
		private IHost _clientHost;
		private FromRtgsSender _fromRtgsSender;
		private IRtgsSubscriber _rtgsSubscriber;

		public GivenOpenConnection(GrpcServerFixture grpcServer)
		{
			_grpcServer = grpcServer;
		}

		public async Task InitializeAsync()
		{
			try
			{
				var rtgsSubscriberOptions = RtgsSubscriberOptions.Builder.CreateNew(_grpcServer.ServerUri)
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices((_, services) => services.AddRtgsPublisher(rtgsSubscriberOptions))
					.Build();

				_fromRtgsSender = _grpcServer.Services.GetRequiredService<FromRtgsSender>();
				_rtgsSubscriber = _clientHost.Services.GetRequiredService<IRtgsSubscriber>();
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
		public async Task WhenReceivedExpectedMessageType_ThenPassToHandlerAndAcknowledge()
		{
			_fromRtgsSender.SetExpectedNumberOfAcknowledgements(1);

			var testHandler = new PayawayFundsHandler();
			_rtgsSubscriber.Start(new[] { testHandler });

			var sentRtgsMessage = await _fromRtgsSender.SendAsync("PayawayFunds", ValidMessages.PayawayFunds);

			_fromRtgsSender.WaitForAcknowledgements();

			using var _ = new AssertionScope();

			_fromRtgsSender.Acknowledgements
				.Should().ContainSingle(acknowledgement => acknowledgement.Header.CorrelationId == sentRtgsMessage.Header.CorrelationId
														   && acknowledgement.Success);

			testHandler.WaitForMessage(WaitForReceivedMessageDuration);

			// TODO: work out how to compare client type with server type?
			//sentRtgsMessage.Should().BeEquivalentTo(handler.ReceivedMessage);
			var sentRtgsMessageJson = JsonSerializer.Serialize(sentRtgsMessage);
			var receivedMessageJson = JsonSerializer.Serialize(testHandler.ReceivedMessage);
			sentRtgsMessageJson.Should().Be(receivedMessageJson);
		}

		[Fact]
		public async Task WhenSubscriberIsStopped_ThenCloseConnection()
		{
			_fromRtgsSender.SetExpectedNumberOfAcknowledgements(1);

			var testHandler = new PayawayFundsHandler();
			_rtgsSubscriber.Start(new[] { testHandler });

			await _fromRtgsSender.SendAsync("PayawayFunds", ValidMessages.PayawayFunds);

			_fromRtgsSender.WaitForAcknowledgements();

			testHandler.WaitForMessage(WaitForReceivedMessageDuration);
			testHandler.Reset();

			await _rtgsSubscriber.StopAsync();

			await _fromRtgsSender.SendAsync("PayawayFunds", ValidMessages.PayawayFunds);

			testHandler.WaitForMessage(WaitForReceivedMessageDuration);

			testHandler.ReceivedMessage.Should().BeNull();
		}

		// TODO: use cancellation tokens to ensure tests would eventually finish by timing out
		private class PayawayFundsHandler : IHandler
		{
			private readonly ManualResetEventSlim _handleSignal = new();

			public RtgsMessage ReceivedMessage { get; private set; }

			public string InstructionType => "PayawayFunds";

			public Task HandleMessageAsync(RtgsMessage message)
			{
				ReceivedMessage = message;
				_handleSignal.Set();

				return Task.CompletedTask;
			}

			public void WaitForMessage(TimeSpan timeout) =>
				_handleSignal.Wait(timeout);

			public void Reset()
			{
				ReceivedMessage = null;
				_handleSignal.Reset();
			}
		}
	}
}
