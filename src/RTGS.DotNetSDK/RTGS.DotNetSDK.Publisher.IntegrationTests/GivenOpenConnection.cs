using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RTGS.DotNetSDK.Publisher.Extensions;
using RTGS.DotNetSDK.Publisher.IntegrationTests.Logging;
using RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;
using RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer;
using RTGS.DotNetSDK.Publisher.Messages;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using Xunit;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class GivenOpenConnection //: IAsyncLifetime, IClassFixture<GrpcServerFixture>
	{
		public class AndShortTestWaitForAcknowledgementDuration : IAsyncLifetime, IClassFixture<GrpcServerFixture>
		{
			private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(0.5);

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
					var rtgsClientOptions = RtgsClientOptions.Builder.CreateNew()
						.BankDid(ValidRequests.BankDid)
						.RemoteHost(_grpcServer.ServerUri.ToString())
						.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
						.Build();

					_clientHost = Host.CreateDefaultBuilder()
						.ConfigureServices((_, services) => services.AddRtgsPublisher(rtgsClientOptions))
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
			public void WhenSendingInParallel_ThenCanSendToRtgs()
			{
				var atomicLockRequests = GenerateFiveUniqueAtomicLockRequests().ToList();

				var sendRequestsSignal = new ManualResetEventSlim();

				var bigMessageTasks = atomicLockRequests
					.Select(request => Task.Run(async () =>
					{
						_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

						sendRequestsSignal.Wait();

						await _rtgsPublisher.SendAtomicLockRequestAsync(request);
					})).ToArray();

				sendRequestsSignal.Set();

				var allCompleted = Task.WaitAll(bigMessageTasks, TimeSpan.FromSeconds(5));
				allCompleted.Should().BeTrue();

				var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();
				receiver.Connections[0].Requests.Select(request => JsonConvert.DeserializeObject<AtomicLockRequest>(request.Data))
					.Should().BeEquivalentTo(atomicLockRequests, options => options.ComparingByMembers<AtomicLockRequest>());

				IEnumerable<AtomicLockRequest> GenerateFiveUniqueAtomicLockRequests()
				{
					// We need writing to the stream to take a significant amount of time to ensure race condition.
					// Using a long end to end id is one way of achieving this.
					yield return new AtomicLockRequest { EndToEndId = new string('a', 100_000) };
					yield return new AtomicLockRequest { EndToEndId = new string('b', 100_000) };
					yield return new AtomicLockRequest { EndToEndId = new string('c', 100_000) };
					yield return new AtomicLockRequest { EndToEndId = new string('d', 100_000) };
					yield return new AtomicLockRequest { EndToEndId = new string('e', 100_000) };
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
			public async Task WhenSendingMessageAndFailedAcknowledgementReceived_ThenLogInformation<TRequest>(PublisherActionWithLogs<TRequest> publisherAction)
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
			[ClassData(typeof(PublisherActionData))]
			public async Task WhenUsingMetadata_ThenSeeBankDidInRequestHeader<TRequest>(PublisherAction<TRequest> publisherAction)
			{
				_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

				await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

				var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

				var connection = receiver.Connections.SingleOrDefault();

				connection.Should().NotBeNull();
				connection!.Headers.Should().ContainSingle(header => header.Key == "bankdid" && header.Value == ValidRequests.BankDid);
			}

			[Theory]
			[ClassData(typeof(PublisherActionWithInstructionTypeData))]
			public async Task ThenCanSendRequestToRtgs<TRequest>(PublisherActionWithInstructionType<TRequest> publisherAction)
			{
				_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

				await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

				var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();
				var receivedMessage = receiver.Connections.Should().ContainSingle().Which.Requests.Should().ContainSingle().Subject;
				var receivedRequest = JsonConvert.DeserializeObject<TRequest>(receivedMessage.Data);

				using var _ = new AssertionScope();

				receivedMessage.Header.Should().NotBeNull();
				receivedMessage.Header?.InstructionType.Should().Be(publisherAction.InstructionType);
				receivedMessage.Header?.CorrelationId.Should().NotBeNullOrEmpty();

				receivedRequest.Should().BeEquivalentTo(publisherAction.Request, options => options.ComparingByMembers<TRequest>());
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
			public async Task WhenBankMessageApiReturnsUnsuccessfulAcknowledgement_ThenReturnServerError<TRequest>(PublisherAction<TRequest> publisherAction)
			{
				_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithFailure());

				var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

				sendResult.Should().Be(SendResult.ServerError);
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
			public async Task WhenBankMessageApiReturnsUnexpectedAcknowledgementBeforeFailureAcknowledgement_ThenReturnServerError<TRequest>(PublisherAction<TRequest> publisherAction)
			{
				_toRtgsMessageHandler.SetupForMessage(handler =>
				{
					handler.ReturnUnexpectedAcknowledgementWithSuccess();
					handler.ReturnExpectedAcknowledgementWithFailure();
				});

				var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

				sendResult.Should().Be(SendResult.ServerError);
			}

			[Theory]
			[ClassData(typeof(PublisherActionData))]
			public async Task WhenBankMessageApiReturnsFailureAcknowledgementBeforeUnexpectedAcknowledgement_ThenReturnServerError<TRequest>(PublisherAction<TRequest> publisherAction)
			{
				_toRtgsMessageHandler.SetupForMessage(handler =>
				{
					handler.ReturnExpectedAcknowledgementWithFailure();
					handler.ReturnUnexpectedAcknowledgementWithSuccess();
				});

				var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

				sendResult.Should().Be(SendResult.ServerError);
			}

			[Theory]
			[ClassData(typeof(PublisherActionData))]
			public async Task WhenBankMessageApiReturnsSuccessWrappedByUnexpectedFailureAcknowledgements_ThenReturnServerError<TRequest>(PublisherAction<TRequest> publisherAction)
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
		}

		public class AndLongTestWaitForAcknowledgementDuration : IAsyncLifetime, IClassFixture<GrpcServerFixture>
		{
			private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(10);

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
					var rtgsClientOptions = RtgsClientOptions.Builder.CreateNew()
						.BankDid(ValidRequests.BankDid)
						.RemoteHost(_grpcServer.ServerUri.ToString())
						.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
						.Build();

					_clientHost = Host.CreateDefaultBuilder()
						.ConfigureServices((_, services) => services.AddRtgsPublisher(rtgsClientOptions))
						.UseSerilog()
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
				using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

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
				using var firstMessageCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
				var firstMessageTask = FluentActions
					.Awaiting(() => publisherAction.InvokeSendDelegateAsync(_rtgsPublisher, firstMessageCancellationTokenSource.Token))
					.Should()
					.ThrowAsync<OperationCanceledException>();

				// Once the server has received the first message we know the semaphore is in use...
				firstMessageReceivedSignal.Wait();

				// ...we can send the second message knowing it will be waiting due to the semaphore.
				using var secondMessageCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
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

				receiver.Connections[0].Requests.Count().Should().Be(1, "the second message should not have been sent as the semaphore should not be entered");
			}
		}
	}
}
