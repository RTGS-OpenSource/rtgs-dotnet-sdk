using System.Collections.Concurrent;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber;

public class GivenOpenConnection : IAsyncLifetime, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(1000);
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
			var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
					TestData.ValidMessages.RtgsGlobalId,
					_grpcServer.ServerUri,
					new Uri("http://id-crypt-cloud-agent-api.com"),
					"id-crypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.Build();

			var idCryptMessageHandler = StatusCodeHttpHandlerBuilderFactory
				.Create()
				.WithOkResponse(GetActiveConnectionWithAlias.HttpRequestResponseContext)
				.WithOkResponse(VerifyPublicSignatureSuccessfully.HttpRequestResponseContext)
				.WithOkResponse(VerifyPrivateSignatureSuccessfully.HttpRequestResponseContext)
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices((_, services) => services
					.AddRtgsSubscriber(rtgsSdkOptions)
					.AddTestIdCryptHttpClient(idCryptMessageHandler))
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
	public async Task WhenUsingMetadata_ThenSeeRtgsGlobalIdInRequestHeader<TRequest>(SubscriberAction<TRequest> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

		subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

		_fromRtgsSender.RequestHeaders.Should().ContainSingle(header => header.Key == "rtgs-global-id"
																		&& header.Value == TestData.ValidMessages.RtgsGlobalId);
	}

	[Theory]
	[ClassData(typeof(SubscriberActionData))]
	public async Task WhenReceivedExpectedMessageType_ThenPassToHandlerAndAcknowledge<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		var sentRtgsMessage = await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, subscriberAction.AdditionalHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		using var _ = new AssertionScope();

		_fromRtgsSender.Acknowledgements
			.Should().ContainSingle(acknowledgement => acknowledgement.CorrelationId == sentRtgsMessage.CorrelationId
													   && acknowledgement.Success);

		subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

		subscriberAction.Handler.ReceivedMessage.Should().BeEquivalentTo(subscriberAction.Message);
	}

	[Theory]
	[ClassData(typeof(SubscriberActionData))]
	public async Task WhenSubscriberIsStopped_ThenCloseConnection<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

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
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

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
	public async Task WhenDisposingInParallel_ThenCanDispose()
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

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
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, subscriberAction.AdditionalHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

		await _rtgsSubscriber.StopAsync();

		var informationLogs = _serilogContext.SubscriberLogs(LogEventLevel.Information);
		informationLogs.Should().BeEquivalentTo(subscriberAction.SubscriberLogs(LogEventLevel.Information), options => options.WithStrictOrdering());
	}

	[Fact]
	public async Task WhenMessageWithNoMessageIdentifierReceived_ThenLogError()
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		await _fromRtgsSender.SendAsync(
			string.Empty,
			TestData.ValidMessages.AtomicLockResponseV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		var errorLogs = _serilogContext.SubscriberLogs(LogEventLevel.Error);
		errorLogs.Should().BeEquivalentTo(new[] { new LogEntry("An error occurred while processing a message (MessageIdentifier: null)", LogEventLevel.Error, typeof(RtgsSubscriberException)) });
	}

	[Fact]
	public async Task WhenMessageWithNoMessageIdentifierReceived_ThenRaiseExceptionEvent()
	{
		Exception raisedException = null;

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

		await _fromRtgsSender.SendAsync(
			string.Empty,
			TestData.ValidMessages.AtomicLockResponseV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		raisedException.Should().BeOfType<RtgsSubscriberException>().Which.Message.Should().Be("Message with no identifier received");
	}

	[Fact]
	public async Task WhenMessageWithNoMessageIdentifierReceived_ThenAcknowledgeAsFailure()
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		var sentRtgsMessage = await _fromRtgsSender.SendAsync(
			string.Empty,
			TestData.ValidMessages.AtomicLockResponseV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		_fromRtgsSender.Acknowledgements
			.Should().ContainSingle(acknowledgement => acknowledgement.CorrelationId == sentRtgsMessage.CorrelationId
													   && !acknowledgement.Success);
	}

	[Fact]
	public async Task WhenMessageWithIdentifierThatCannotBeHandledReceived_ThenLogError()
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		await _fromRtgsSender.SendAsync(
			"cannot be handled",
			TestData.ValidMessages.AtomicLockResponseV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		var errorLogs = _serilogContext.SubscriberLogs(LogEventLevel.Error);
		errorLogs.Should().BeEquivalentTo(new[] { new LogEntry("An error occurred while processing a message (MessageIdentifier: cannot be handled)", LogEventLevel.Error, typeof(RtgsSubscriberException)) });
	}

	[Fact]
	public async Task WhenMessageWithIdentifierThatCannotBeHandledReceived_ThenRaiseExceptionEvent()
	{
		Exception raisedException = null;

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

		await _fromRtgsSender.SendAsync(
			"cannot be handled",
			TestData.ValidMessages.AtomicLockResponseV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		raisedException.Should().BeOfType<RtgsSubscriberException>().Which.Message.Should().Be("No handler found for message");
	}

	[Fact]
	public async Task WhenMessageWithIdentifierThatCannotBeHandledReceived_ThenAcknowledgeAsFailure()
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		var sentRtgsMessage = await _fromRtgsSender.SendAsync(
			"cannot be handled",
			TestData.ValidMessages.AtomicLockResponseV1);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		_fromRtgsSender.Acknowledgements
			.Should().ContainSingle(acknowledgement => acknowledgement.CorrelationId == sentRtgsMessage.CorrelationId
													   && !acknowledgement.Success);
	}

	[Theory]
	[ClassData(typeof(SubscriberActionData))]
	public async Task WhenMessageWithIdentifierThatCannotBeHandledReceived_ThenSubsequentMessagesCanBeHandled<TRequest>(SubscriberAction<TRequest> subscriberAction)
	{
		_fromRtgsSender.SetExpectedAcknowledgementCount(2);

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		await _fromRtgsSender.SendAsync(
			"cannot be handled",
			TestData.ValidMessages.AtomicLockResponseV1);

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

		await _rtgsSubscriber.StartAsync(testHandlers);
		_rtgsSubscriber.OnExceptionOccurred += (_, args) => exceptionSignal.Set();

		await _fromRtgsSender.SendAsync("MessageRejected", TestData.ValidMessages.MessageRejected);

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

		await _rtgsSubscriber.StartAsync(testHandlers);
		_rtgsSubscriber.OnExceptionOccurred += (_, args) =>
		{
			actualRaisedException = args.Exception;
			exceptionSignal.Set();
		};

		await _fromRtgsSender.SendAsync("MessageRejected", TestData.ValidMessages.MessageRejected);

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

		await _rtgsSubscriber.StartAsync(testHandlers);

		_fromRtgsSender.SetExpectedAcknowledgementCount(2);

		await _fromRtgsSender.SendAsync("MessageRejected", TestData.ValidMessages.MessageRejected);

		var signingHeaders = new Dictionary<string, string>()
		{
			{ "public-did-signature", "public-did-signature" },
			{ "pairwise-did-signature", "pairwise-did-signature" },
			{ "alias", "alias" }
		};

		await _fromRtgsSender.SendAsync("PayawayFunds", TestData.ValidMessages.PayawayFunds, signingHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		var payawayFundsHandler = testHandlers.OfType<AllTestHandlers.TestPayawayFundsV1Handler>().Single();
		payawayFundsHandler.ReceivedMessage.Should().BeEquivalentTo(TestData.ValidMessages.PayawayFunds);
	}

	[Theory]
	[ClassData(typeof(SubscriberActionData))]
	public async Task AndSubscriberIsStopped_WhenStarting_ThenReceiveMessages<TRequest>(SubscriberAction<TRequest> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		await _rtgsSubscriber.StopAsync();

		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		var sentRtgsMessage = await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		using var _ = new AssertionScope();

		_fromRtgsSender.Acknowledgements
			.Should().ContainSingle(acknowledgement => acknowledgement.CorrelationId == sentRtgsMessage.CorrelationId
													   && acknowledgement.Success);

		subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

		subscriberAction.Handler.ReceivedMessage.Should().BeEquivalentTo(subscriberAction.Message);
	}

	[Fact]
	public async Task WhenExceptionEventHandlerThrows_ThenLogError()
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		_rtgsSubscriber.OnExceptionOccurred += (_, _) => throw new InvalidOperationException("test");

		await _fromRtgsSender.SendAsync("will-throw", TestData.ValidMessages.AtomicLockResponseV1);

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

		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		_rtgsSubscriber.OnExceptionOccurred += (_, _) => throw new InvalidOperationException("test");

		await _fromRtgsSender.SendAsync("will-throw", TestData.ValidMessages.AtomicLockResponseV1);

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

		subscriberAction.Handler.ReceivedMessage.Should().BeEquivalentTo(subscriberAction.Message);
	}

	[Theory]
	[ClassData(typeof(SubscriberActionData))]
	public async Task AndMessageIsBeingProcessed_WhenStopping_ThenHandleGracefully<TRequest>(SubscriberAction<TRequest> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

		await _rtgsSubscriber.StopAsync();

		_serilogContext.SubscriberLogs(LogEventLevel.Error).Should().BeEmpty();
	}

	[Theory]
	[ClassData(typeof(SubscriberActionData))]
	public async Task AndMessageIsBeingProcessed_WhenDisposing_ThenHandleGracefully<TRequest>(SubscriberAction<TRequest> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

		await _rtgsSubscriber.DisposeAsync();

		_serilogContext.SubscriberLogs(LogEventLevel.Error).Should().BeEmpty();
	}

	[Fact]
	public void WhenStartingInParallel_ThenOnlyOneSucceeds()
	{
		using var startSignal = new ManualResetEventSlim();

		var invalidOperationExceptions = new ConcurrentBag<InvalidOperationException>();

		const int concurrentStartThreadsCount = 20;
		var startTasks = Enumerable.Range(1, concurrentStartThreadsCount)
			.Select(_ => Task.Run(async () =>
			{
				startSignal.Wait();

				try
				{
					await _rtgsSubscriber.StartAsync(new AllTestHandlers());
				}
				catch (InvalidOperationException ex)
				{
					invalidOperationExceptions.Add(ex);
				}
			}))
			.ToArray();

		startSignal.Set();

		Task.WaitAll(startTasks, TimeSpan.FromSeconds(5));

		invalidOperationExceptions.Count.Should().Be(concurrentStartThreadsCount - 1);
	}

	[Fact]
	public async Task WhenStoppingInParallel_ThenAllSucceed()
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		using var stopSignal = new ManualResetEventSlim();

		var invalidOperationExceptions = new ConcurrentBag<InvalidOperationException>();

		const int concurrentStopThreadsCount = 20;
		var stopTasks = Enumerable.Range(1, concurrentStopThreadsCount)
			.Select(_ => Task.Run(async () =>
			{
				stopSignal.Wait();

				try
				{
					await _rtgsSubscriber.StopAsync();
				}
				catch (InvalidOperationException ex)
				{
					invalidOperationExceptions.Add(ex);
				}
			}))
			.ToArray();

		stopSignal.Set();

		Task.WaitAll(stopTasks, TimeSpan.FromSeconds(5));

		invalidOperationExceptions.Should().BeEmpty();
	}
}
