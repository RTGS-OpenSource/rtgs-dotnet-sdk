using System.Text.Json;
using System.Text.Json.Serialization;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.Signing.GivenOpenConnection;

public class WhenSigningIsSuccessful : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);
	private static readonly Uri IdCryptApiUri = new("http://id-crypt-cloud-agent-api.com");
	private const string IdCryptApiKey = "id-crypt-api-key";

	private readonly GrpcServerFixture _grpcServer;

	private IRtgsPublisher _rtgsPublisher;
	private ToRtgsMessageHandler _toRtgsMessageHandler;
	private StatusCodeHttpHandler _idCryptMessageHandler;
	private IHost _clientHost;

	public WhenSigningIsSuccessful(GrpcServerFixture grpcServer)
	{
		_grpcServer = grpcServer;

		SetupSerilogLogger();

		SetupDependencies();
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
					new Uri("http://id-crypt-cloud-agent-api.com"),
					"id-crypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
				.KeepAlivePingDelay(TimeSpan.FromSeconds(30))
				.KeepAlivePingTimeout(TimeSpan.FromSeconds(30))
				.Build();

			_idCryptMessageHandler = StatusCodeHttpHandlerBuilderFactory
				.Create()
				.WithOkResponse(GetActiveConnectionWithAlias.HttpRequestResponseContext)
				.WithOkResponse(SignDocument.HttpRequestResponseContext)
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices(services => services
					.AddRtgsPublisher(rtgsSdkOptions)
					.AddTestIdCryptHttpClient(_idCryptMessageHandler))
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
	public async Task WhenCallingIdCryptAgent_ThenBaseAddressIsExpected<TRequest>(PublisherAction<TRequest> publisherAction)
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		var actualApiUri = _idCryptMessageHandler.Requests[SignDocument.Path]
			.Single()
			.RequestUri
			!.GetLeftPart(UriPartial.Authority);

		actualApiUri.Should().BeEquivalentTo(IdCryptApiUri.GetLeftPart(UriPartial.Authority));
	}

	[Theory]
	[ClassData(typeof(PublisherActionSignedMessagesData))]
	public async Task WhenCallingIdCryptAgent_ThenApiKeyHeaderIsExpected<TRequest>(PublisherAction<TRequest> publisherAction)
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

		_idCryptMessageHandler.Requests[SignDocument.Path]
			.Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(IdCryptApiKey);
	}

	[Theory]
	[ClassData(typeof(PublisherActionSignedMessagesData))]
	public async Task WhenCallingIdCryptAgent_ThenSignDocumentIsCalled<TRequest>(PublisherAction<TRequest> publisherAction)
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

		_idCryptMessageHandler.Requests.Should().ContainKey("/json-signatures/sign");
	}

	[Theory]
	[ClassData(typeof(PublisherActionSignedMessagesData))]
	public async Task WhenCallingIdCryptAgent_ThenConnectionIdIsInBody<TRequest>(PublisherAction<TRequest> publisherAction)
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

		var requestContent = await _idCryptMessageHandler.Requests[SignDocument.Path]
			.Single().Content.ReadAsStringAsync();

		var signDocumentRequest = JsonSerializer.Deserialize<SignDocumentRequest<TRequest>>(
			requestContent,
			new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		signDocumentRequest.ConnectionId.Should().Be(GetActiveConnectionWithAlias.ExpectedResponse.ConnectionId);
	}

	[Theory]
	[ClassData(typeof(PublisherActionSignedMessagesData))]
	public async Task WhenCallingIdCryptAgent_ThenMessageContentsAreInBody<TRequest>(PublisherAction<TRequest> publisherAction)
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

		var requestContent = await _idCryptMessageHandler.Requests[SignDocument.Path]
			.Single().Content.ReadAsStringAsync();

		var signDocumentRequest = JsonSerializer.Deserialize<SignDocumentRequest<TRequest>>(requestContent);

		signDocumentRequest.Document.Should().BeEquivalentTo(publisherAction.Request, options => options.ComparingByMembers<TRequest>());
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
			{ "pairwise-did-signature", SignDocument.Response.PairwiseDidSignature },
			{ "public-did-signature", SignDocument.Response.PublicDidSignature },
			{ "alias", TestData.ValidMessages.IdCryptAlias }
		};

		receivedMessage.Headers.Should().BeEquivalentTo(expectedHeaders);
	}

	private record SignDocumentRequest<TDocument>
	{
		[JsonPropertyName("connection_id")]
		public string ConnectionId { get; init; }

		[JsonPropertyName("document")]
		public TDocument Document { get; init; }
	}
}
