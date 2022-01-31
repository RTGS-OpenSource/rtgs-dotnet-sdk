using Newtonsoft.Json;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests;

public class GivenOpenConnection
{
	public class AndShortTestWaitForAcknowledgementDuration : IAsyncLifetime, IClassFixture<GrpcServerFixture>
	{
		private const string BankPartnerDid = "bank-partner-did";
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;

		private IRtgsPublisher _rtgsPublisher;
		private ToRtgsMessageHandler _toRtgsMessageHandler;
		private IHost _clientHost;

		public AndShortTestWaitForAcknowledgementDuration(GrpcServerFixture grpcServer)
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
				var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(
						ValidMessages.BankDid, 
						_grpcServer.ServerUri,
						"",
						"",
						"")
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.KeepAlivePingDelay(TimeSpan.FromSeconds(30))
					.KeepAlivePingTimeout(TimeSpan.FromSeconds(30))
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services.AddRtgsPublisher(rtgsPublisherOptions))
					.UseSerilog()
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
		public async Task WhenDisposingInParallel_ThenCanDispose()
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequestV1(), BankPartnerDid);

			using var disposeSignal = new ManualResetEventSlim();
			const int concurrentDisposableThreads = 20;
			var disposeTasks = Enumerable.Range(1, concurrentDisposableThreads)
				.Select(request => Task.Run(async () =>
				{
					disposeSignal.Wait();

					await _rtgsPublisher.DisposeAsync();
				}))
				.ToArray();

			disposeSignal.Set();

			var allCompleted = Task.WaitAll(disposeTasks, TimeSpan.FromSeconds(5));
			allCompleted.Should().BeTrue();
		}

		[Fact]
		public void WhenSendingInParallel_ThenCanSendToRtgs()
		{
			var atomicLockRequests = GenerateFiveUniqueAtomicLockRequests().ToList();

			using var sendRequestsSignal = new ManualResetEventSlim();

			var bigRequestTasks = atomicLockRequests
				.Select(request => Task.Run(async () =>
				{
					_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

					sendRequestsSignal.Wait();

					await _rtgsPublisher.SendAtomicLockRequestAsync(request, BankPartnerDid);
				})).ToArray();

			sendRequestsSignal.Set();

			var allCompleted = Task.WaitAll(bigRequestTasks, TimeSpan.FromSeconds(5));
			allCompleted.Should().BeTrue();

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();
			receiver.Connections.Single().Requests.Select(request => JsonConvert.DeserializeObject<AtomicLockRequestV1>(request.Data))
				.Should().BeEquivalentTo(atomicLockRequests, options => options.ComparingByMembers<AtomicLockRequestV1>());

			IEnumerable<AtomicLockRequestV1> GenerateFiveUniqueAtomicLockRequests()
			{
				// We need writing to the stream to take a significant amount of time to ensure race condition.
				// Using a long end to end id is one way of achieving this.
				yield return new AtomicLockRequestV1 { EndToEndId = new string('a', 100_000) };
				yield return new AtomicLockRequestV1 { EndToEndId = new string('b', 100_000) };
				yield return new AtomicLockRequestV1 { EndToEndId = new string('c', 100_000) };
				yield return new AtomicLockRequestV1 { EndToEndId = new string('d', 100_000) };
				yield return new AtomicLockRequestV1 { EndToEndId = new string('e', 100_000) };
			}
		}

		[Theory]
		[ClassData(typeof(PublisherActionSuccessAcknowledgementLogsData))]
		public async Task WhenSendingMessageAndSuccessAcknowledgementReceived_ThenLogInformation<TRequest>(PublisherActionWithLogs<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());

			await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			using var _ = new AssertionScope();

			var informationLogs = _serilogContext.PublisherLogs(LogEventLevel.Information);
			informationLogs.Should().BeEquivalentTo(publisherAction.PublisherLogs(LogEventLevel.Information), options => options.WithStrictOrdering());

			var warningLogs = _serilogContext.PublisherLogs(LogEventLevel.Warning);
			warningLogs.Should().BeEmpty();

			var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
			errorLogs.Should().BeEmpty();
		}

		[Theory]
		[ClassData(typeof(PublisherActionFailedAcknowledgementLogsData))]
		public async Task WhenSendingMessageAndFailedAcknowledgementReceived_ThenLog<TRequest>(PublisherActionWithLogs<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithFailure());

			await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			using var _ = new AssertionScope();

			var informationLogs = _serilogContext.PublisherLogs(LogEventLevel.Information);
			informationLogs.Should().BeEquivalentTo(publisherAction.PublisherLogs(LogEventLevel.Information), options => options.WithStrictOrdering());

			var warningLogs = _serilogContext.PublisherLogs(LogEventLevel.Warning);
			warningLogs.Should().BeEmpty();

			var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
			errorLogs.Should().BeEquivalentTo(publisherAction.PublisherLogs(LogEventLevel.Error), options => options.WithStrictOrdering());
		}

		[Theory]
		[ClassData(typeof(PublisherActionRpcExceptionLogsData))]
		public async Task WhenSendingMessageAndRpcExceptionReceived_ThenLog<TRequest>(PublisherActionWithLogs<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ThrowRpcException(StatusCode.Unavailable, "test"));

			await FluentActions.Awaiting(() => publisherAction.InvokeSendDelegateAsync(_rtgsPublisher))
				.Should()
				.ThrowAsync<RpcException>();

			using var _ = new AssertionScope();

			var informationLogs = _serilogContext.PublisherLogs(LogEventLevel.Information);
			informationLogs.Should().BeEquivalentTo(publisherAction.PublisherLogs(LogEventLevel.Information), options => options.WithStrictOrdering());

			var warningLogs = _serilogContext.PublisherLogs(LogEventLevel.Warning);
			warningLogs.Should().BeEmpty();

			var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
			errorLogs.Should().BeEquivalentTo(publisherAction.PublisherLogs(LogEventLevel.Error), options => options.WithStrictOrdering());
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task AndDisposedPublisher_WhenSending_ThenThrow<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			await _rtgsPublisher.DisposeAsync();

			await FluentActions
				.Awaiting(() => publisherAction.InvokeSendDelegateAsync(_rtgsPublisher))
				.Should()
				.ThrowAsync<ObjectDisposedException>()
				.WithMessage("*RtgsPublisher*");
		}

		[Theory]
		[ClassData(typeof(PublisherActionWithNullMessagesData))]
		public async Task AndMessageIsNull_WhenSending_ThenThrow<TRequest>(PublisherAction<TRequest> publisherAction) =>
			await FluentActions
				.Awaiting(() => publisherAction.InvokeSendDelegateAsync(_rtgsPublisher))
				.Should()
				.ThrowAsync<ArgumentNullException>()
				.WithMessage("Value cannot be null. (Parameter 'message')");

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenUsingMetadata_ThenSeeBankDidInRequestHeader<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			var connection = receiver.Connections.SingleOrDefault();

			connection.Should().NotBeNull();
			connection!.Headers.Should().ContainSingle(header => header.Key == "bankdid" && header.Value == ValidMessages.BankDid);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenUsingRtgsMessageHeaders_ThenSeeHeadersInMessageHeaders<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			var receivedMessage = receiver.Connections.Should().ContainSingle().Which.Requests.Should().ContainSingle().Subject;
			receivedMessage.Headers.Should().BeEquivalentTo(publisherAction.Headers);
		}

		[Theory]
		[ClassData(typeof(PublisherActionWithMessageIdentifierData))]
		public async Task ThenCanSendRequestToRtgs<TRequest>(PublisherActionWithMessageIdentifier<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();
			var receivedMessage = receiver.Connections.Should().ContainSingle().Which.Requests.Should().ContainSingle().Subject;

			using var _ = new AssertionScope();

			receivedMessage.MessageIdentifier.Should().Be(publisherAction.MessageIdentifier);
			receivedMessage.CorrelationId.Should().NotBeNullOrEmpty();

			if (publisherAction.ComparePayload)
			{
				var receivedRequest = JsonConvert.DeserializeObject<TRequest>(receivedMessage.Data);
				receivedRequest.Should().BeEquivalentTo(publisherAction.Request, options => options.ComparingByMembers<TRequest>());
			}
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgement_ThenReturnSuccess<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.Success);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenBankMessageApiReturnsUnsuccessfulAcknowledgement_ThenReturnRejected<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithFailure());

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.Rejected);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgementTooLate_ThenReturnTimeout<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1))));

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);
			sendResult.Should().Be(SendResult.Timeout);
		}

		[Theory]
		[ClassData(typeof(PublisherActionTimeoutAcknowledgementLogsData))]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgementTooLate_ThenLogError<TRequest>(PublisherActionWithLogs<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1))));

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
			errorLogs.Should().BeEquivalentTo(publisherAction.PublisherLogs(LogEventLevel.Error), options => options.WithStrictOrdering());
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenSendingMultipleMessages_ThenOnlyOneConnection<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnUnexpectedAcknowledgementWithSuccess());
			await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnUnexpectedAcknowledgementWithSuccess());
			await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			receiver.NumberOfConnections.Should().Be(1);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenSendingMultipleMessagesAndLastOneTimesOut_ThenDoNotSeePreviousSuccess<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithSuccess());
			var sendResult1 = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1))));
			var sendResult2 = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);
			using var _ = new AssertionScope();

			sendResult1.Should().Be(SendResult.Success);
			sendResult2.Should().Be(SendResult.Timeout);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenBankMessageApiOnlyReturnsUnexpectedAcknowledgement_ThenReturnTimeout<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
				handler.ReturnUnexpectedAcknowledgementWithSuccess());

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.Timeout);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenBankMessageApiReturnsUnexpectedAcknowledgementBeforeFailureAcknowledgement_ThenReturnRejected<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
			{
				handler.ReturnUnexpectedAcknowledgementWithSuccess();
				handler.ReturnExpectedAcknowledgementWithFailure();
			});

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.Rejected);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenBankMessageApiReturnsFailureAcknowledgementBeforeUnexpectedAcknowledgement_ThenReturnRejected<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
			{
				handler.ReturnExpectedAcknowledgementWithFailure();
				handler.ReturnUnexpectedAcknowledgementWithSuccess();
			});

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.Rejected);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenBankMessageApiReturnsSuccessWrappedByUnexpectedFailureAcknowledgements_ThenReturnSuccess<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler =>
			{
				handler.ReturnUnexpectedAcknowledgementWithFailure();
				handler.ReturnExpectedAcknowledgementWithSuccess();
				handler.ReturnUnexpectedAcknowledgementWithFailure();
			});

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.Success);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenBankMessageApiReturnsSuccessForSecondMessageOnly_ThenDoNotTimeout<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			var sendResult1 = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);
			sendResult1.Should().Be(SendResult.Timeout);

			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			var sendResult2 = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);
			sendResult2.Should().Be(SendResult.Success);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenBankMessageApiThrowsExceptionForFirstMessage_ThenStillHandleSecondMessage<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ThrowRpcException(StatusCode.Unknown, "test"));

			await FluentActions.Awaiting(() => publisherAction.InvokeSendDelegateAsync(_rtgsPublisher))
				.Should()
				.ThrowAsync<RpcException>();

			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);
			sendResult.Should().Be(SendResult.Success);
		}
	}

	public class AndLongTestWaitForAcknowledgementDuration : IAsyncLifetime, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(30);
		private static readonly TimeSpan TestWaitForSendDuration = TimeSpan.FromSeconds(15);

		private readonly GrpcServerFixture _grpcServer;

		private IRtgsPublisher _rtgsPublisher;
		private IHost _clientHost;

		public AndLongTestWaitForAcknowledgementDuration(GrpcServerFixture grpcServer)
		{
			_grpcServer = grpcServer;
		}

		public async Task InitializeAsync()
		{
			try
			{
				var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(
						ValidMessages.BankDid, 
						_grpcServer.ServerUri,
						"",
						"",
						"")
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices(services => services.AddRtgsPublisher(rtgsPublisherOptions))
					.Build();

				_rtgsPublisher = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
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

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenCancellationTokenIsCancelledBeforeAcknowledgmentTimeout_ThenThrowOperationCancelled<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			using var cancellationTokenSource = new CancellationTokenSource(TestWaitForSendDuration);

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();
			receiver.RegisterOnMessageReceived(() => cancellationTokenSource.Cancel());

			await FluentActions.Awaiting(() => publisherAction.InvokeSendDelegateAsync(_rtgsPublisher, cancellationTokenSource.Token))
				.Should().ThrowAsync<OperationCanceledException>();
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenCancellationTokenIsCancelledBeforeSemaphoreIsEntered_ThenThrowOperationCancelled<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			using var firstMessageReceivedSignal = new ManualResetEventSlim();
			receiver.RegisterOnMessageReceived(() => firstMessageReceivedSignal.Set());

			// Send the first message that has no acknowledgement setup so the client
			// will hold on to the semaphore for a long time.
			using var firstMessageCancellationTokenSource = new CancellationTokenSource(TestWaitForSendDuration);
			var firstMessageTask = FluentActions
				.Awaiting(() => publisherAction.InvokeSendDelegateAsync(_rtgsPublisher, firstMessageCancellationTokenSource.Token))
				.Should()
				.ThrowAsync<OperationCanceledException>();

			// Once the server has received the first message we know the semaphore is in use...
			firstMessageReceivedSignal.Wait(TestWaitForSendDuration);

			// ...we can send the second message knowing it will be waiting due to the semaphore.
			using var secondMessageCancellationTokenSource = new CancellationTokenSource(TestWaitForSendDuration);
			var secondMessageTask = FluentActions
				.Awaiting(() => publisherAction.InvokeSendDelegateAsync(_rtgsPublisher, secondMessageCancellationTokenSource.Token))
				.Should()
				.ThrowAsync<OperationCanceledException>();

			// While the first message's acknowledgement is still being waited, cancel the second message before it is sent.
			secondMessageCancellationTokenSource.Cancel();
			await secondMessageTask;

			// Allow the test to gracefully stop.
			firstMessageCancellationTokenSource.Cancel();
			await firstMessageTask;

			receiver.Connections.Single().Requests.Count().Should().Be(1, "the second message should not have been sent as the semaphore should not be entered");
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenDisposingBeforeAcknowledgmentTimeout_ThenThrowOperationCancelled<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			using var messageReceivedSignal = new ManualResetEventSlim();
			receiver.RegisterOnMessageReceived(() => messageReceivedSignal.Set());

			var messageTask = FluentActions
				.Awaiting(() => publisherAction.InvokeSendDelegateAsync(_rtgsPublisher))
				.Should()
				.ThrowAsync<OperationCanceledException>();

			messageReceivedSignal.Wait(TestWaitForSendDuration);

			await _rtgsPublisher.DisposeAsync();

			await messageTask;
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenDisposingBeforeSemaphoreIsEntered_ThenThrowOperationCancelled<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			using var firstMessageReceivedSignal = new ManualResetEventSlim();
			receiver.RegisterOnMessageReceived(() => firstMessageReceivedSignal.Set());

			// Send the first message that has no acknowledgement setup so the client
			// will hold on to the semaphore for a long time.
			using var firstMessageCancellationTokenSource = new CancellationTokenSource(TestWaitForSendDuration);
			var firstMessageTask = FluentActions
				.Awaiting(() => publisherAction.InvokeSendDelegateAsync(_rtgsPublisher, firstMessageCancellationTokenSource.Token))
				.Should()
				.ThrowAsync<OperationCanceledException>();

			// Once the server has received the first message we know the semaphore is in use...
			firstMessageReceivedSignal.Wait(TestWaitForSendDuration);

			// ...we can send the second message knowing it will be waiting due to the semaphore.
			using var secondMessageCancellationTokenSource = new CancellationTokenSource(TestWaitForSendDuration);
			var secondMessageTask = FluentActions
				.Awaiting(() => publisherAction.InvokeSendDelegateAsync(_rtgsPublisher, secondMessageCancellationTokenSource.Token))
				.Should()
				.ThrowAsync<OperationCanceledException>();

			await _rtgsPublisher.DisposeAsync();

			// Release the semaphore for other threads
			firstMessageCancellationTokenSource.Cancel();

			// Allow the test to gracefully stop.
			await firstMessageTask;
			await secondMessageTask;

			using var _ = new AssertionScope();

			receiver.Connections.Count.Should().Be(1, "the second call to send a message should not open another connection when it is being disposed");
			receiver.Connections.SelectMany(connection => connection.Requests).Count().Should().Be(1, "the second message should not have been sent as the semaphore should not be entered");
		}
	}
}
