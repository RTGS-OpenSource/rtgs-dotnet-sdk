using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RTGS.DotNetSDK.Publisher.Extensions;
using RTGS.DotNetSDK.Publisher.IntegrationTests.Logging;
using RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;
using RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class GivenOpenConnection : IAsyncLifetime, IClassFixture<GrpcServerFixture>
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(0.5);

		private readonly GrpcServerFixture _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;

		private IRtgsPublisher _rtgsPublisher;
		private ToRtgsMessageHandler _toRtgsMessageHandler;
		private IHost _clientHost;

		public GivenOpenConnection(GrpcServerFixture grpcServer)
		{
			_grpcServer = grpcServer;

			SetupSerilogLogger();

			_serilogContext = TestCorrelator.CreateContext();
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

		private static void SetupSerilogLogger() =>
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.WriteTo.TestCorrelator()
				.CreateLogger();

		[Theory]
		[ClassData(typeof(PublisherActionSuccessAcknowledgementLogsData))]
		public async Task WhenSendingMessageAndSuccessAcknowledgementReceived_ThenLogInformation<TRequest>(PublisherActionWithLogs<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();

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
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithFailure();

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
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();

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
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();

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
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.Success);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenBankMessageApiReturnsUnsuccessfulAcknowledgement_ThenReturnServerError<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithFailure();

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.ServerError);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgementTooLate_ThenReturnTimeout<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1)));

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.Timeout);
		}

		[Theory]
		[ClassData(typeof(PublisherActionTimeoutAcknowledgementLogsData))]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgementTooLate_ThenLogError<TRequest>(PublisherActionWithLogs<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1)));

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
			errorLogs.Should().BeEquivalentTo(publisherAction.PublisherLogs(LogEventLevel.Error), options => options.WithStrictOrdering());
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenSendingMultipleMessages_ThenOnlyOneConnection<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();
			await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();
			await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();
			await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

			receiver.NumberOfConnections.Should().Be(1);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenSendingMultipleMessagesAndLastOneTimesOut_ThenDoNotSeePreviousSuccess<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();
			var sendResult1 = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1)));
			var sendResult2 = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);
			using var _ = new AssertionScope();

			sendResult1.Should().Be(SendResult.Success);
			sendResult2.Should().Be(SendResult.Timeout);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenBankMessageApiOnlyReturnsUnexpectedAcknowledgement_ThenReturnTimeout<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueUnexpectedAcknowledgementWithSuccess();

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.Timeout);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenBankMessageApiReturnsUnexpectedAcknowledgementBeforeFailureAcknowledgement_ThenReturnServerError<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueUnexpectedAcknowledgementWithSuccess();
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithFailure();

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.ServerError);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenBankMessageApiReturnsFailureAcknowledgementBeforeUnexpectedAcknowledgement_ThenReturnServerError<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithFailure();
			_toRtgsMessageHandler.EnqueueUnexpectedAcknowledgementWithSuccess();

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.ServerError);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenBankMessageApiReturnsSuccessWrappedByUnexpectedFailureAcknowledgements_ThenReturnServerError<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueUnexpectedAcknowledgementWithFailure();
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();
			_toRtgsMessageHandler.EnqueueUnexpectedAcknowledgementWithFailure();

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.Success);
		}

		[Theory]
		[ClassData(typeof(PublisherActionData))]
		public async Task WhenCancellationTokenIsCancelled_ThenReturnOperationCancelled<TRequest>(PublisherAction<TRequest> publisherAction) =>
			await FluentActions.Awaiting(async () =>
				{
					using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(0.4));
					return await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher, cancellationTokenSource.Token);
				})
				.Should().ThrowAsync<OperationCanceledException>();
	}
}
