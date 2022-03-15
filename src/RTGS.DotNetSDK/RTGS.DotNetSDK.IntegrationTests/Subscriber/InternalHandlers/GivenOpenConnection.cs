using Microsoft.AspNetCore.WebUtilities;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.Publisher.HttpHandlers;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using ValidMessages = RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData.ValidMessages;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalHandlers;

public class GivenOpenConnection : IAsyncLifetime, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(100);
	private static readonly TimeSpan WaitForAcknowledgementsDuration = TimeSpan.FromMilliseconds(100);

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;
	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;
	private readonly StatusCodeHttpHandler _idCryptMessageHandler;
	private readonly List<IHandler> _allTestHandlers = new AllTestHandlers().ToList();
	private readonly AllTestHandlers.TestIdCryptCreateInvitationNotificationV1 _invitationNotificationHandler;

	public GivenOpenConnection(GrpcServerFixture grpcServer)
	{
		_grpcServer = grpcServer;

		SetupSerilogLogger();

		_serilogContext = TestCorrelator.CreateContext();
		
		_idCryptMessageHandler = new StatusCodeHttpHandler(IdCryptEndPoints.MockHttpResponses);
		_invitationNotificationHandler = _allTestHandlers.OfType<AllTestHandlers.TestIdCryptCreateInvitationNotificationV1>().Single();
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
					TestData.ValidMessages.BankDid,
					_grpcServer.ServerUri,
					new Uri("http://id-crypt-cloud-agent-api.com"),
					"id-crypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices((_, services) => services
					.AddRtgsSubscriber(rtgsSdkOptions)
					.AddTestIdCryptHttpClient(_idCryptMessageHandler))
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

	[Fact]
	public async Task WhenUsingMetadata_ThenSeeBankDidInRequestHeader()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);
	
		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);
	
		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);
	
		_fromRtgsSender.RequestHeaders.Should().ContainSingle(header => header.Key == "bankdid" && header.Value == ValidMessages.BankDid);
	}

	[Fact]
	public async Task WhenReceivedExpectedMessageType_ThenPassToHandlerAndAcknowledge()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		var receivedMessage = ValidMessages.IdCryptCreateInvitationRequestV1;

		var sentRtgsMessage = await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", receivedMessage);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		using var _ = new AssertionScope();

		_fromRtgsSender.Acknowledgements
			.Should().ContainSingle(acknowledgement => acknowledgement.CorrelationId == sentRtgsMessage.CorrelationId
													   && acknowledgement.Success);
		
		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);
		
		var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[IdCryptEndPoints.InvitationPath].RequestUri?.Query);
		var alias = inviteRequestQueryParams["alias"];

		var message = new IdCryptCreateInvitationNotificationV1
		{
			Alias = alias,
			ConnectionId = IdCryptTestMessages.ConnectionInviteResponse.ConnectionID,
			PartnerBankDid = receivedMessage.PartnerBankDid
		};
		
		_invitationNotificationHandler.ReceivedMessage.Should().BeEquivalentTo(message);
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

	[Theory]
	[ClassData(typeof(SubscriberActionWithLogsData))]
	public async Task WhenMessageReceived_ThenLogInformation<TMessage>(SubscriberActionWithLogs<TMessage> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

		await _rtgsSubscriber.StopAsync();

		var informationLogs = _serilogContext.SubscriberLogs(LogEventLevel.Information);
		informationLogs.Should().BeEquivalentTo(subscriberAction.SubscriberLogs(LogEventLevel.Information), options => options.WithStrictOrdering());
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
}
