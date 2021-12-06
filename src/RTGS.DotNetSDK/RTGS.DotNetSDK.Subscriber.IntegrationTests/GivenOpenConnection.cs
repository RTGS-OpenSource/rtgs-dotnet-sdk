using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.DotNetSDK.Subscriber.Extensions;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestServer;
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

		[Theory]
		[ClassData(typeof(SubscriberActionData))]
		public async Task WhenReceivedExpectedMessageType_ThenPassToHandlerAndAcknowledge<TMessage>(SubscriberAction<TMessage> subscriberAction)
		{
			_fromRtgsSender.SetExpectedNumberOfAcknowledgements(1);

			_rtgsSubscriber.Start(new[] { subscriberAction.Handler });

			var sentRtgsMessage = await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

			_fromRtgsSender.WaitForAcknowledgements();

			using var _ = new AssertionScope();

			_fromRtgsSender.Acknowledgements
				.Should().ContainSingle(acknowledgement => acknowledgement.Header.CorrelationId == sentRtgsMessage.Header.CorrelationId
														   && acknowledgement.Success);

			subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

			subscriberAction.Handler.ReceivedMessage.Should().BeEquivalentTo(subscriberAction.Message);
		}

		[Theory]
		[ClassData(typeof(SubscriberActionData))]
		public async Task WhenSubscriberIsStopped_ThenCloseConnection<TMessage>(SubscriberAction<TMessage> subscriberAction)
		{
			_fromRtgsSender.SetExpectedNumberOfAcknowledgements(1);

			_rtgsSubscriber.Start(new[] { subscriberAction.Handler });

			await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

			_fromRtgsSender.WaitForAcknowledgements();

			subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);
			subscriberAction.Handler.Reset();

			await _rtgsSubscriber.StopAsync();

			await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

			subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

			subscriberAction.Handler.ReceivedMessage.Should().BeNull();
		}
	}
}
