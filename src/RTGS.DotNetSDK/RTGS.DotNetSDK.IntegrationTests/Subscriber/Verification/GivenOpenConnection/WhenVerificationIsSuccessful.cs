using System.Text.Json;
using System.Text.Json.Serialization;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.Verification.GivenOpenConnection;

public class WhenVerificationIsSuccessful : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly Uri IdCryptApiUri = new("http://id-crypt-cloud-agent-api.com");
	private const string IdCryptApiKey = "id-crypt-api-key";

	private static readonly TimeSpan WaitForAcknowledgementsDuration = TimeSpan.FromMilliseconds(100);
	private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(100);

	private readonly GrpcServerFixture _grpcServer;
	private StatusCodeHttpHandler _idCryptMessageHandler;
	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;

	public WhenVerificationIsSuccessful(GrpcServerFixture grpcServer)
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
					TestData.ValidMessages.BankDid,
					_grpcServer.ServerUri,
					new Uri("http://id-crypt-cloud-agent-api.com"),
					"id-crypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.Build();

			_idCryptMessageHandler = StatusCodeHttpHandlerBuilderFactory
				.Create()
				.WithOkResponse(GetActiveConnectionWithAlias.HttpRequestResponseContext)
				.WithOkResponse(VerifyPrivateSignatureSuccessfully.HttpRequestResponseContext)
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
	public async Task ThenVerifyDocumentIsCalled<TRequest>(SubscriberAction<TRequest> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, subscriberAction.AdditionalHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

		await _rtgsSubscriber.StopAsync();

		_idCryptMessageHandler.Requests.Should().ContainKey("/json-signatures/verify/connection-did");
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task ThenBaseAddressIsExpected<TRequest>(SubscriberAction<TRequest> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, subscriberAction.AdditionalHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

		await _rtgsSubscriber.StopAsync();

		var actualVerifyPrivateSignatureApiUri = _idCryptMessageHandler.Requests[VerifyPrivateSignatureSuccessfully.Path]
			.Single()
			.RequestUri
			!.GetLeftPart(UriPartial.Authority);

		actualVerifyPrivateSignatureApiUri.Should().BeEquivalentTo(IdCryptApiUri.GetLeftPart(UriPartial.Authority));
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task ThenApiKeyHeaderIsExpected<TRequest>(SubscriberAction<TRequest> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, subscriberAction.AdditionalHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

		await _rtgsSubscriber.StopAsync();

		_idCryptMessageHandler.Requests[VerifyPrivateSignatureSuccessfully.Path]
			.Single()
			.Headers.GetValues("X-API-Key")
			.Should().ContainSingle()
			.Which.Should().Be(IdCryptApiKey);
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task WhenCallingVerifyPrivateSignature_ThenConnectionIdIsInBody<TRequest>(SubscriberAction<TRequest> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, subscriberAction.AdditionalHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

		await _rtgsSubscriber.StopAsync();

		var requestContent = await _idCryptMessageHandler.Requests[VerifyPrivateSignatureSuccessfully.Path]
			.Single().Content.ReadAsStringAsync();

		var signDocumentRequest = JsonSerializer.Deserialize<VerifyPrivateSignatureRequest<TRequest>>(requestContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		signDocumentRequest.ConnectionId.Should().Be(GetActiveConnectionWithAlias.ExpectedResponse.ConnectionId);
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task WhenCallingVerifyPrivateSignature_ThenPublicSignatureIsInBody<TRequest>(SubscriberAction<TRequest> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, subscriberAction.AdditionalHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

		await _rtgsSubscriber.StopAsync();

		var requestContent = await _idCryptMessageHandler.Requests[VerifyPrivateSignatureSuccessfully.Path]
			.Single().Content.ReadAsStringAsync();

		var signDocumentRequest = JsonSerializer.Deserialize<VerifyPrivateSignatureRequest<TRequest>>(requestContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		var expectedPrivateSignature = subscriberAction.AdditionalHeaders["pairwise-did-signature"];

		signDocumentRequest.Signature.Should().Be(expectedPrivateSignature);
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task WhenCallingVerifyPrivateSignature_ThenMessageContentsAreInBody<TRequest>(SubscriberAction<TRequest> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, subscriberAction.AdditionalHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

		await _rtgsSubscriber.StopAsync();

		var requestContent = await _idCryptMessageHandler.Requests[VerifyPrivateSignatureSuccessfully.Path]
			.Single().Content.ReadAsStringAsync();

		var signDocumentRequest = JsonSerializer.Deserialize<VerifyPrivateSignatureRequest<TRequest>>(requestContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		signDocumentRequest.Document.Should().BeEquivalentTo(subscriberAction.Message);
	}

	private record VerifyPrivateSignatureRequest<TDocument>
	{
		[JsonPropertyName("connection_id")]
		public string ConnectionId { get; init; }

		[JsonPropertyName("document")]
		public TDocument Document { get; init; }

		[JsonPropertyName("signature")]
		public string Signature { get; init; }
	}
}
