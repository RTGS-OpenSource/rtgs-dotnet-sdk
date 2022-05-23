using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.Signing.GivenOpenConnection;

public sealed class WhenSigningIsSuccessful : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);
	private static readonly Uri IdCryptServiceUri = new("https://id-crypt-service");

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;

	private RtgsSdkOptions _rtgsSdkOptions;
	private StatusCodeHttpHandler _idCryptServiceMessageHandler;
	private IHost _clientHost;
	private IRtgsPublisher _rtgsPublisher;
	private ToRtgsMessageHandler _toRtgsMessageHandler;

	public WhenSigningIsSuccessful(GrpcServerFixture grpcServer)
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
			_rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
					TestData.ValidMessages.RtgsGlobalId,
					_grpcServer.ServerUri,
					IdCryptServiceUri)
				.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
				.KeepAlivePingDelay(TimeSpan.FromSeconds(30))
				.KeepAlivePingTimeout(TimeSpan.FromSeconds(30))
				.EnableMessageSigning()
				.Build();

			_idCryptServiceMessageHandler = StatusCodeHttpHandlerBuilderFactory
				.Create()
				.WithOkResponse(SignMessage.HttpRequestResponseContext)
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices(services => services
					.AddRtgsPublisher(_rtgsSdkOptions)
					.AddTestIdCryptServiceHttpClient(_idCryptServiceMessageHandler))
				.UseSerilog()
				.Build();

			_rtgsPublisher = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
			_toRtgsMessageHandler = _grpcServer.Services.GetRequiredService<ToRtgsMessageHandler>();
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
	[ClassData(typeof(PublisherActionSignedMessagesData))]
	public async Task WhenCallingIdCryptService_ThenBaseAddressIsExpected<TRequest>(PublisherAction<TRequest> publisherAction)
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

		var actualApiUri = _idCryptServiceMessageHandler.Requests[SignMessage.Path]
			.Single()
			.RequestUri
			!.GetLeftPart(UriPartial.Authority);

		actualApiUri.Should().BeEquivalentTo(IdCryptServiceUri.GetLeftPart(UriPartial.Authority));
	}

	[Theory]
	[ClassData(typeof(PublisherActionSignedMessagesData))]
	public async Task WhenCallingIdCryptService_ThenPathIsExpected<TRequest>(PublisherAction<TRequest> publisherAction)
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

		_idCryptServiceMessageHandler.Requests.Should().ContainKey("/api/message/sign");
	}

	[Theory]
	[ClassData(typeof(PublisherActionSignedMessagesData))]
	public async Task WhenCallingIdCryptAgent_ThenMessageContentsAreInBody<TRequest>(PublisherAction<TRequest> publisherAction)
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

		var requestContent = await _idCryptServiceMessageHandler
			.Requests[SignMessage.Path].Single()
			.Content!.ReadAsStringAsync();

		requestContent.Should().BeEquivalentTo(publisherAction.SerialisedSignedDocument);
	}

	[Theory]
	[ClassData(typeof(PublisherActionSignedMessagesData))]
	public async Task WhenSendingMessage_ThenSignaturesAndAliasAreInMessageHeaders<TRequest>(PublisherAction<TRequest> publisherAction)
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		var receivedMessage = receiver.Connections.Should().ContainSingle().Which.Requests.Should().ContainSingle().Subject;

		var expectedHeaders = new Dictionary<string, string>
		{
			{ "pairwise-did-signature", SignMessage.Response.PairwiseDidSignature },
			{ "public-did-signature", SignMessage.Response.PublicDidSignature },
			{ "alias", TestData.ValidMessages.IdCryptAlias },
			{ "from-rtgs-global-id", _rtgsSdkOptions.RtgsGlobalId },
		};

		// using contain here because for some messages other headers are sent (e.g. rtgs-global-id)
		receivedMessage.Headers.Should().Contain(expectedHeaders);
	}

	[Theory]
	[ClassData(typeof(PublisherActionSignedMessagesData))]
	public async Task ThenPublisherLogs<TRequest>(PublisherAction<TRequest> publisherAction)
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

		using var _ = new AssertionScope();

		var expectedInformationLogs = new List<LogEntry>
		{
			new($"Signing {typeof(TRequest).Name} message", LogEventLevel.Information),
			new($"Signed {typeof(TRequest).Name} message", LogEventLevel.Information)
		};

		_serilogContext.PublisherLogs(LogEventLevel.Information)
			.Should().StartWith(expectedInformationLogs);

		_serilogContext.PublisherLogs(LogEventLevel.Warning).Should().BeEmpty();

		_serilogContext.PublisherLogs(LogEventLevel.Error).Should().BeEmpty();

	}

	[Theory]
	[ClassData(typeof(PublisherActionSignedMessagesData))]
	public async Task ThenIdCryptServiceClientLogs<TRequest>(PublisherAction<TRequest> publisherAction)
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

		using var _ = new AssertionScope();

		var expectedDebugLogs = new List<LogEntry>
		{
			new("Sending SignMessage request to ID Crypt Service", LogEventLevel.Debug),
			new("Sent SignMessage request to ID Crypt Service", LogEventLevel.Debug)
		};

		_serilogContext.LogsFor("RTGS.DotNetSDK.IdCrypt.IdCryptServiceClient", LogEventLevel.Debug)
			.Should().BeEquivalentTo(expectedDebugLogs, options => options.WithStrictOrdering());

		_serilogContext.LogsFor("RTGS.DotNetSDK.IdCrypt.IdCryptServiceClient", LogEventLevel.Warning)
			.Should().BeEmpty();

		_serilogContext.LogsFor("RTGS.DotNetSDK.IdCrypt.IdCryptServiceClient", LogEventLevel.Error)
			.Should().BeEmpty();
	}
}
