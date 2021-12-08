using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.DotNetSDK.Subscriber.Extensions;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestServer;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using Xunit;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests
{
	public class GivenOpenConnection : IAsyncLifetime, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(100);
		private static readonly TimeSpan WaitForAcknowledgementsDuration = TimeSpan.FromMilliseconds(100);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;
		private IHost _clientHost;
		private FromRtgsSender _fromRtgsSender;
		private IRtgsSubscriber _rtgsSubscriber;

		public GivenOpenConnection(GrpcServerFixture grpcServer)
		{
			_grpcServer = grpcServer;

			SetupSerilogLogger();

			_serilogContext = TestCorrelator.CreateContext();
		}

		private static void SetupSerilogLogger() =>
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.WriteTo.TestCorrelator()
				.CreateLogger();

		public async Task InitializeAsync()
		{
			try
			{
				var rtgsSubscriberOptions = RtgsSubscriberOptions.Builder.CreateNew(_grpcServer.ServerUri)
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices((_, services) => services.AddRtgsSubscriber(rtgsSubscriberOptions))
					.UseSerilog()
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

			_rtgsSubscriber.Start(subscriberAction.AllTestHandlers);

			var sentRtgsMessage = await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

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

			_rtgsSubscriber.Start(subscriberAction.AllTestHandlers);

			await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

			await _rtgsSubscriber.StopAsync();

			subscriberAction.Handler.Reset();

			await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

			subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

			subscriberAction.Handler.ReceivedMessage.Should().BeNull();
		}

		[Theory]
		[ClassData(typeof(SubscriberActionData))]
		public async Task WhenSubscriberIsDisposed_ThenCloseConnection<TMessage>(SubscriberAction<TMessage> subscriberAction)
		{
			_fromRtgsSender.SetExpectedNumberOfAcknowledgements(1);

			_rtgsSubscriber.Start(subscriberAction.AllTestHandlers);

			await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

			await _rtgsSubscriber.DisposeAsync();

			subscriberAction.Handler.Reset();

			await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

			subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

			subscriberAction.Handler.ReceivedMessage.Should().BeNull();
		}

		[Fact]
		public void WhenDisposingInParallel_ThenCanDispose()
		{
			_rtgsSubscriber.Start(new AllTestHandlers());

			using var disposeSignal = new ManualResetEventSlim();
			const int concurrentDisposableThreads = 20;
			var disposeTasks = Enumerable.Range(1, concurrentDisposableThreads)
				.Select(request => Task.Run(async () =>
				{
					disposeSignal.Wait();

					await _rtgsSubscriber.DisposeAsync();
				}))
				.ToArray();

			disposeSignal.Set();

			var allCompleted = Task.WaitAll(disposeTasks, TimeSpan.FromSeconds(5));
			allCompleted.Should().BeTrue();
		}

		[Theory]
		[ClassData(typeof(SubscriberActionData))]
		public async Task WhenMessageReceived_ThenLogInformation<TMessage>(SubscriberAction<TMessage> subscriberAction)
		{
			_rtgsSubscriber.Start(subscriberAction.AllTestHandlers);

			await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

			subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

			await _rtgsSubscriber.StopAsync();

			// TODO: check logs
		}
	}
}
