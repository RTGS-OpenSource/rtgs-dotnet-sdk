using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.DotNetSDK.Subscriber.Exceptions;
using RTGS.DotNetSDK.Subscriber.Extensions;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.Logging;
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
		private static readonly TimeSpan WaitForExceptionEventDuration = TimeSpan.FromMilliseconds(100);

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
				var rtgsSubscriberOptions = RtgsSubscriberOptions.Builder.CreateNew(ValidMessages.BankDid, _grpcServer.ServerUri)
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
		public async Task WhenUsingMetadata_ThenSeeBankDidInRequestHeader<TRequest>(SubscriberAction<TRequest> subscriberAction)
		{
			_rtgsSubscriber.Start(subscriberAction.AllTestHandlers);

			await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

			subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

			_fromRtgsSender.RequestHeaders.Should().ContainSingle(header => header.Key == "bankdid" && header.Value == ValidMessages.BankDid);
		}

		[Theory]
		[ClassData(typeof(SubscriberActionData))]
		public async Task WhenReceivedExpectedMessageType_ThenPassToHandlerAndAcknowledge<TMessage>(SubscriberAction<TMessage> subscriberAction)
		{
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
		[ClassData(typeof(SubscriberActionWithLogsData))]
		public async Task WhenMessageReceived_ThenLogInformation<TMessage>(SubscriberActionWithLogs<TMessage> subscriberAction)
		{
			_rtgsSubscriber.Start(subscriberAction.AllTestHandlers);

			await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

			await _rtgsSubscriber.StopAsync();

			var informationLogs = _serilogContext.SubscriberLogs(LogEventLevel.Information);
			informationLogs.Should().BeEquivalentTo(subscriberAction.SubscriberLogs(LogEventLevel.Information), options => options.WithStrictOrdering());
		}

		[Fact]
		public async Task WhenMessageWithNoHeaderReceived_ThenLogError()
		{
			_rtgsSubscriber.Start(new AllTestHandlers());

			await _fromRtgsSender.SendAsync(
				"will not be used as the header is being set to null",
				ValidMessages.AtomicLockResponseV1,
				message => message.Header = null);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			await _rtgsSubscriber.StopAsync();

			var errorLogs = _serilogContext.SubscriberLogs(LogEventLevel.Error);
			errorLogs.Should().BeEquivalentTo(new[] { new LogEntry("An error occurred while processing a message (MessageIdentifier: null)", LogEventLevel.Error, typeof(RtgsSubscriberException)) });
		}

		[Fact]
		public async Task WhenMessageWithNoHeaderReceived_ThenRaiseExceptionEvent()
		{
			Exception raisedException = null;

			_rtgsSubscriber.Start(new AllTestHandlers());
			_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

			await _fromRtgsSender.SendAsync(
				"will not be used as the header is being set to null",
				ValidMessages.AtomicLockResponseV1,
				message => message.Header = null);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			await _rtgsSubscriber.StopAsync();

			raisedException.Should().BeOfType<RtgsSubscriberException>().Which.Message.Should().Be("Message with no header received");
		}

		[Fact]
		public async Task WhenMessageWithNoHeaderReceived_ThenAcknowledgeAsFailure()
		{
			_rtgsSubscriber.Start(new AllTestHandlers());

			await _fromRtgsSender.SendAsync(
				"will not be used as the header is being set to null",
				ValidMessages.AtomicLockResponseV1,
				message => message.Header = null);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			await _rtgsSubscriber.StopAsync();

			_fromRtgsSender.Acknowledgements
				.Should().ContainSingle(acknowledgement => acknowledgement.Header != null
														   && acknowledgement.Header.CorrelationId == string.Empty
														   && acknowledgement.Header.InstructionType == string.Empty
														   && !acknowledgement.Success);
		}

		[Fact]
		public async Task WhenMessageWithNoMessageIdentifierReceived_ThenLogError()
		{
			_rtgsSubscriber.Start(new AllTestHandlers());

			await _fromRtgsSender.SendAsync(
				string.Empty,
				ValidMessages.AtomicLockResponseV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			await _rtgsSubscriber.StopAsync();

			var errorLogs = _serilogContext.SubscriberLogs(LogEventLevel.Error);
			errorLogs.Should().BeEquivalentTo(new[] { new LogEntry("An error occurred while processing a message (MessageIdentifier: null)", LogEventLevel.Error, typeof(RtgsSubscriberException)) });
		}

		[Fact]
		public async Task WhenMessageWithNoMessageIdentifierReceived_ThenRaiseExceptionEvent()
		{
			Exception raisedException = null;

			_rtgsSubscriber.Start(new AllTestHandlers());
			_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

			await _fromRtgsSender.SendAsync(
				string.Empty,
				ValidMessages.AtomicLockResponseV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			await _rtgsSubscriber.StopAsync();

			raisedException.Should().BeOfType<RtgsSubscriberException>().Which.Message.Should().Be("Message with no identifier received");
		}

		[Fact]
		public async Task WhenMessageWithNoMessageIdentifierReceived_ThenAcknowledgeAsFailure()
		{
			_rtgsSubscriber.Start(new AllTestHandlers());

			var sentRtgsMessage = await _fromRtgsSender.SendAsync(
				string.Empty,
				ValidMessages.AtomicLockResponseV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			await _rtgsSubscriber.StopAsync();

			_fromRtgsSender.Acknowledgements
				.Should().ContainSingle(acknowledgement => acknowledgement.Header != null
														   && acknowledgement.Header.CorrelationId == sentRtgsMessage.Header.CorrelationId
														   && acknowledgement.Header.InstructionType == string.Empty
														   && !acknowledgement.Success);
		}

		[Fact]
		public async Task WhenMessageWithIdentifierThatCannotBeHandledReceived_ThenLogError()
		{
			_rtgsSubscriber.Start(new AllTestHandlers());

			await _fromRtgsSender.SendAsync(
				"cannot be handled",
				ValidMessages.AtomicLockResponseV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			await _rtgsSubscriber.StopAsync();

			var errorLogs = _serilogContext.SubscriberLogs(LogEventLevel.Error);
			errorLogs.Should().BeEquivalentTo(new[] { new LogEntry("An error occurred while processing a message (MessageIdentifier: cannot be handled)", LogEventLevel.Error, typeof(RtgsSubscriberException)) });
		}

		[Fact]
		public async Task WhenMessageWithIdentifierThatCannotBeHandledReceived_ThenRaiseExceptionEvent()
		{
			Exception raisedException = null;

			_rtgsSubscriber.Start(new AllTestHandlers());
			_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

			await _fromRtgsSender.SendAsync(
				"cannot be handled",
				ValidMessages.AtomicLockResponseV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			await _rtgsSubscriber.StopAsync();

			raisedException.Should().BeOfType<RtgsSubscriberException>().Which.Message.Should().Be("No handler found for message");
		}

		[Fact]
		public async Task WhenMessageWithIdentifierThatCannotBeHandledReceived_ThenAcknowledgeAsFailure()
		{
			_rtgsSubscriber.Start(new AllTestHandlers());

			var sentRtgsMessage = await _fromRtgsSender.SendAsync(
				"cannot be handled",
				ValidMessages.AtomicLockResponseV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			await _rtgsSubscriber.StopAsync();

			_fromRtgsSender.Acknowledgements
				.Should().ContainSingle(acknowledgement => acknowledgement.Header != null
														   && acknowledgement.Header.CorrelationId == sentRtgsMessage.Header.CorrelationId
														   && acknowledgement.Header.InstructionType == "cannot be handled"
														   && !acknowledgement.Success);
		}

		[Theory]
		[ClassData(typeof(SubscriberActionData))]
		public async Task WhenMessageWithIdentifierThatCannotBeHandledReceived_ThenSubsequentMessagesCanBeHandled<TRequest>(SubscriberAction<TRequest> subscriberAction)
		{
			_fromRtgsSender.SetExpectedAcknowledgementCount(2);

			_rtgsSubscriber.Start(new AllTestHandlers());

			await _fromRtgsSender.SendAsync(
				"cannot be handled",
				ValidMessages.AtomicLockResponseV1);

			await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

			subscriberAction.Handler.ReceivedMessage.Should().BeEquivalentTo(subscriberAction.Message);

			await _rtgsSubscriber.StopAsync();
		}

		[Fact]
		public async Task WhenHandlerThrows_ThenLogError()
		{
			using var exceptionSignal = new ManualResetEventSlim();

			var testHandlers = new AllTestHandlers()
				.ThrowWhenMessageRejectV1Received(new OutOfMemoryException("test"));

			_rtgsSubscriber.Start(testHandlers);
			_rtgsSubscriber.OnExceptionOccurred += (_, args) => exceptionSignal.Set();

			await _fromRtgsSender.SendAsync("MessageRejected", ValidMessages.MessageRejected);

			exceptionSignal.Wait(WaitForExceptionEventDuration);

			var errorLogs = _serilogContext.SubscriberLogs(LogEventLevel.Error);
			errorLogs.Should().BeEquivalentTo(new[]
			{
				new LogEntry(
					"An error occurred while handling a message (MessageIdentifier: MessageRejected)",
					LogEventLevel.Error,
					typeof(OutOfMemoryException))
			});
		}

		[Fact]
		public async Task WhenHandlerThrows_ThenRaiseExceptionEvent()
		{
			using var exceptionSignal = new ManualResetEventSlim();
			Exception actualRaisedException = null;

			var expectedRaisedException = new OutOfMemoryException("test");

			var testHandlers = new AllTestHandlers()
				.ThrowWhenMessageRejectV1Received(expectedRaisedException);

			_rtgsSubscriber.Start(testHandlers);
			_rtgsSubscriber.OnExceptionOccurred += (_, args) =>
			{
				actualRaisedException = args.Exception;
				exceptionSignal.Set();
			};

			await _fromRtgsSender.SendAsync("MessageRejected", ValidMessages.MessageRejected);

			exceptionSignal.Wait(WaitForExceptionEventDuration);

			await _rtgsSubscriber.StopAsync();

			actualRaisedException.Should().BeSameAs(expectedRaisedException);
		}

		[Fact]
		public async Task WhenHandlerThrowsForFirstMessage_ThenSecondMessageIsHandled()
		{
			var testHandlers = new AllTestHandlers()
				.ThrowWhenMessageRejectV1Received(new OutOfMemoryException("test"))
				.ToList();

			_rtgsSubscriber.Start(testHandlers);

			_fromRtgsSender.SetExpectedAcknowledgementCount(2);

			await _fromRtgsSender.SendAsync("MessageRejected", ValidMessages.MessageRejected);

			await _fromRtgsSender.SendAsync("PayawayFunds", ValidMessages.PayawayFunds);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			await _rtgsSubscriber.StopAsync();

			var payawayFundsHandler = testHandlers.OfType<AllTestHandlers.TestPayawayFundsV1Handler>().Single();
			payawayFundsHandler.ReceivedMessage.Should().BeEquivalentTo(ValidMessages.PayawayFunds);
		}

		[Fact]
		public async Task AndSubscriberHasBeenDisposed_WhenStarting_ThenThrow()
		{
			_rtgsSubscriber.Start(new AllTestHandlers());

			await _rtgsSubscriber.DisposeAsync();

			FluentActions.Invoking(() => _rtgsSubscriber.Start(new AllTestHandlers()))
				.Should().ThrowExactly<ObjectDisposedException>()
				.WithMessage("*RtgsSubscriber*");
		}

		[Fact]
		public async Task AndSubscriberHasBeenDisposed_WhenStopping_ThenThrow()
		{
			_rtgsSubscriber.Start(new AllTestHandlers());

			await _rtgsSubscriber.DisposeAsync();

			await FluentActions.Awaiting(() => _rtgsSubscriber.StopAsync())
				.Should().ThrowExactlyAsync<ObjectDisposedException>()
				.WithMessage("*RtgsSubscriber*");
		}

		[Theory]
		[ClassData(typeof(SubscriberActionData))]
		public async Task AndSubscriberIsStopped_WhenStarting_ThenReceivedMessages<TRequest>(SubscriberAction<TRequest> subscriberAction)
		{
			_rtgsSubscriber.Start(subscriberAction.AllTestHandlers);

			await _rtgsSubscriber.StopAsync();

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

		[Fact]
		public async Task WhenExceptionEventHandlerThrows_ThenLogError()
		{
			_rtgsSubscriber.Start(new AllTestHandlers());

			_rtgsSubscriber.OnExceptionOccurred += (_, _) => throw new InvalidOperationException("test");

			await _fromRtgsSender.SendAsync("will-throw", ValidMessages.AtomicLockResponseV1);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			await _rtgsSubscriber.StopAsync();

			var errorLogs = _serilogContext.SubscriberLogs(LogEventLevel.Error);
			errorLogs.Should().BeEquivalentTo(new[]
			{
				new LogEntry(
					"An error occurred while processing a message (MessageIdentifier: will-throw)",
					LogEventLevel.Error,
					typeof(RtgsSubscriberException)),
				new LogEntry(
					"An error occurred while raising exception occurred event",
					LogEventLevel.Error,
					typeof(InvalidOperationException))
			});
		}

		[Theory]
		[ClassData(typeof(SubscriberActionData))]
		public async Task WhenExceptionEventHandlerThrows_ThenSubsequentMessagesCanBeHandled<TRequest>(SubscriberAction<TRequest> subscriberAction)
		{
			_fromRtgsSender.SetExpectedAcknowledgementCount(2);

			_rtgsSubscriber.Start(new AllTestHandlers());

			_rtgsSubscriber.OnExceptionOccurred += (_, _) => throw new InvalidOperationException("test");

			await _fromRtgsSender.SendAsync("will-throw", ValidMessages.AtomicLockResponseV1);

			await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

			_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

			subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

			subscriberAction.Handler.ReceivedMessage.Should().BeEquivalentTo(subscriberAction.Message);
		}
	}
}
