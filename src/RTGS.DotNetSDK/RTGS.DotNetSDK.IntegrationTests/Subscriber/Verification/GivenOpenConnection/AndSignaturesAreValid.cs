using System.Text.Json;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;
using RTGS.IDCrypt.Service.Contracts.Message.Verify;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.Verification.GivenOpenConnection;

public sealed class AndSignaturesAreValid : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly Uri IdCryptServiceUri = new("https://id-crypt-service");

	private static readonly TimeSpan WaitForAcknowledgementsDuration = TimeSpan.FromMilliseconds(100);
	private static readonly TimeSpan WaitForReceivedRequestDuration = TimeSpan.FromMilliseconds(100);

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;

	private StatusCodeHttpHandler _idCryptServiceHttpHandler;
	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;

	public AndSignaturesAreValid(GrpcServerFixture grpcServer)
	{
		_grpcServer = grpcServer;

		SetupSerilogLogger();

		SetupDependencies();

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

	private void SetupDependencies()
	{
		try
		{
			var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
					TestData.ValidMessages.RtgsGlobalId,
					_grpcServer.ServerUri,
					IdCryptServiceUri)
				.EnableMessageSigning()
				.Build();

			_idCryptServiceHttpHandler = StatusCodeHttpHandlerBuilderFactory
				.Create()
				.WithOkResponse(VerifyMessageSuccessfully.HttpRequestResponseContext)
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices((_, services) => services
					.AddRtgsSubscriber(rtgsSdkOptions)
					.AddTestIdCryptServiceHttpClient(_idCryptServiceHttpHandler))
				.UseSerilog()
				.Build();

			_fromRtgsSender = _grpcServer.Services.GetRequiredService<FromRtgsSender>();
			_rtgsSubscriber = _clientHost.Services.GetRequiredService<IRtgsSubscriber>();
		}
		catch (Exception)
		{
			Dispose();

			throw;
		}
	}

	public void Dispose()
	{
		_clientHost?.Dispose();

		_grpcServer.Reset();
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task WhenVerifyingMessage_ThenPathIsExpected<TRequest>(SubscriberAction<TRequest> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message,
			subscriberAction.AdditionalHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		_idCryptServiceHttpHandler.WaitForRequests(WaitForReceivedRequestDuration);

		await _rtgsSubscriber.StopAsync();

		_idCryptServiceHttpHandler.Requests.Should().ContainKey("/api/message/verify");
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task WhenCallingVerifyMessage_ThenBaseAddressIsExpected<TRequest>(
		SubscriberAction<TRequest> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message,
			subscriberAction.AdditionalHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		_idCryptServiceHttpHandler.WaitForRequests(WaitForReceivedRequestDuration);

		await _rtgsSubscriber.StopAsync();

		var actualVerifyPrivateSignatureApiUri = _idCryptServiceHttpHandler.Requests[VerifyMessageSuccessfully.Path]
			.Single()
			.RequestUri
			!.GetLeftPart(UriPartial.Authority);

		actualVerifyPrivateSignatureApiUri.Should().BeEquivalentTo(IdCryptServiceUri.GetLeftPart(UriPartial.Authority));
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task WhenCallingVerifyMessage_ThenBodyIsExpected<TRequest, TVerifyMessage>(
		VerifiableSubscriberAction<TRequest, TVerifyMessage> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(new AllTestHandlers());

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message,
			subscriberAction.AdditionalHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		_idCryptServiceHttpHandler.WaitForRequests(WaitForReceivedRequestDuration);

		await _rtgsSubscriber.StopAsync();

		var requestContent = await _idCryptServiceHttpHandler.Requests[VerifyMessageSuccessfully.Path]
			.Single().Content!.ReadAsStringAsync();

		var signDocumentRequest = JsonSerializer.Deserialize<VerifyRequest>(requestContent);

		signDocumentRequest.Should().BeEquivalentTo(
			new VerifyRequest
			{
				RtgsGlobalId = subscriberAction.AdditionalHeaders["from-rtgs-global-id"],
				PrivateSignature = subscriberAction.AdditionalHeaders["pairwise-did-signature"],
				Alias = subscriberAction.AdditionalHeaders["alias"]
			}, options => options.Excluding(x => x.Message));

		var expectedMessage = JsonSerializer.Serialize(subscriberAction.VerifyMessage);
		var actualMessage = JsonSerializer.Serialize(signDocumentRequest!.Message);
		actualMessage!.Should().BeEquivalentTo(expectedMessage);
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesWithLogsData))]
	public async Task WhenMessageReceived_ThenLogInformation<TMessage>(
		SubscriberActionWithLogs<TMessage> subscriberAction)
	{
		var allHandlers = new AllTestHandlers();

		await _rtgsSubscriber.StartAsync(allHandlers);

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message,
			subscriberAction.AdditionalHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		var handler = allHandlers.OfType<AllTestHandlers.TestHandler<TMessage>>().Single();
		handler.WaitForMessage(WaitForReceivedRequestDuration);

		await _rtgsSubscriber.StopAsync();

		var informationLogs = _serilogContext.LogsForNamespace("RTGS.DotNetSDK.Subscriber", LogEventLevel.Information);

		informationLogs.Should().BeEquivalentTo(subscriberAction.SubscriberLogs(LogEventLevel.Information),
			options => options.WithStrictOrdering());
	}
}
