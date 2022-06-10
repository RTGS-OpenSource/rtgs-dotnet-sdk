using System.Text.Json;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.InternalMessages;
using RTGS.IDCrypt.Service.Contracts.Message.Verify;
using ValidMessages = RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData.ValidMessages;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalHandlers.GivenPartnerBankEarmarkFundsV1SentToOpenSubscriberConnection;

public sealed class AndSignaturesAreValid : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly Uri IdCryptServiceUri = new("https://id-crypt-service");
	private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(1_000);
	private static readonly TimeSpan WaitForSubscriberAcknowledgementDuration = TimeSpan.FromMilliseconds(100);

	private readonly List<IHandler> _allTestHandlers = new AllTestHandlers().ToList();

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;

	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;
	private ToRtgsMessageHandler _toRtgsMessageHandler;
	private StatusCodeHttpHandler _idCryptServiceHttpHandler;

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
					ValidMessages.RtgsGlobalId,
					_grpcServer.ServerUri,
					new("https://id-crypt-service"))
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

	[Fact]
	public async Task WhenMessageReceived_ThenSeeRtgsGlobalIdInRequestHeader()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync(nameof(PartnerBankEarmarkFundsV1), ValidMessages.PartnerBankEarmarkFundsV1, SubscriberActions.DefaultSigningHeaders);

		_fromRtgsSender.RequestHeaders.Should().ContainSingle(header =>
			header.Key == "rtgs-global-id"
			&& header.Value == ValidMessages.RtgsGlobalId);
	}

	[Fact]
	public async Task WhenMessageReceived_ThenAcknowledge()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		var sentRtgsMessage = await _fromRtgsSender.SendAsync(nameof(PartnerBankEarmarkFundsV1), ValidMessages.PartnerBankEarmarkFundsV1, SubscriberActions.DefaultSigningHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		_fromRtgsSender.Acknowledgements.Should().ContainSingle(acknowledgement =>
			acknowledgement.CorrelationId == sentRtgsMessage.CorrelationId && acknowledgement.Success);
	}

	[Fact]
	public async Task WhenMessageReceived_ThenHandledByEarmarkFundsV1Handler()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync(nameof(PartnerBankEarmarkFundsV1), ValidMessages.PartnerBankEarmarkFundsV1, SubscriberActions.DefaultSigningHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		var handler = _allTestHandlers.GetHandler<EarmarkFundsV1>();
		handler.WaitForMessage(WaitForReceivedMessageDuration);

		handler.ReceivedMessage.Should().BeEquivalentTo(new EarmarkFundsV1
		{
			LckId = ValidMessages.PartnerBankEarmarkFundsV1.LckId,
			Amt = ValidMessages.PartnerBankEarmarkFundsV1.CdtrAmt,
			Acct = ValidMessages.PartnerBankEarmarkFundsV1.DbtrAgntAcct
		});

		await _rtgsSubscriber.StopAsync();
	}

	[Fact]
	public async Task WhenMessageReceived_ThenLogInformation()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync(nameof(PartnerBankEarmarkFundsV1), ValidMessages.PartnerBankEarmarkFundsV1, SubscriberActions.DefaultSigningHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		var handler = _allTestHandlers.OfType<AllTestHandlers.TestHandler<EarmarkFundsV1>>().Single();
		handler.WaitForMessage(WaitForReceivedMessageDuration);

		var informationLogs = _serilogContext.LogsForNamespace("RTGS.DotNetSDK.Subscriber", LogEventLevel.Information);
		var expectedLogs = new List<LogEntry>
		{
			new("RTGS Subscriber started", LogEventLevel.Information),
			new("PartnerBankEarmarkFundsV1 message received from RTGS", LogEventLevel.Information),
			new("Verifying PartnerBankEarmarkFundsV1 message", LogEventLevel.Information),
			new("Verified PartnerBankEarmarkFundsV1 message", LogEventLevel.Information),
			new("RTGS Subscriber stopping", LogEventLevel.Information),
			new("RTGS Subscriber stopped", LogEventLevel.Information)
		};
		await _rtgsSubscriber.StopAsync();
		informationLogs.Should().BeEquivalentTo(expectedLogs, options => options.WithStrictOrdering());
	}

	[Fact]
	public async Task WhenVerifyingMessage_ThenPathIsExpected()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync(nameof(PartnerBankEarmarkFundsV1), ValidMessages.PartnerBankEarmarkFundsV1, SubscriberActions.DefaultSigningHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		await _rtgsSubscriber.StopAsync();

		_idCryptServiceHttpHandler.Requests.Should().ContainKey("/api/message/verify");
	}

	[Fact]
	public async Task WhenCallingVerifyMessage_ThenBaseAddressIsExpected()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync(nameof(PartnerBankEarmarkFundsV1), ValidMessages.PartnerBankEarmarkFundsV1, SubscriberActions.DefaultSigningHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		await _rtgsSubscriber.StopAsync();

		var actualVerifyPrivateSignatureApiUri = _idCryptServiceHttpHandler.Requests[VerifyMessageSuccessfully.Path]
			.Single()
			.RequestUri
			!.GetLeftPart(UriPartial.Authority);

		actualVerifyPrivateSignatureApiUri.Should().BeEquivalentTo(IdCryptServiceUri.GetLeftPart(UriPartial.Authority));
	}

	[Fact]
	public async Task WhenCallingVerifyMessage_ThenBodyIsExpected()
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync(nameof(PartnerBankEarmarkFundsV1), ValidMessages.PartnerBankEarmarkFundsV1, SubscriberActions.DefaultSigningHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForSubscriberAcknowledgementDuration);

		await _rtgsSubscriber.StopAsync();

		var requestContent = await _idCryptServiceHttpHandler.Requests[VerifyMessageSuccessfully.Path]
			.Single().Content!.ReadAsStringAsync();

		var signDocumentRequest = JsonSerializer.Deserialize<VerifyRequest>(requestContent);

		var expectedMessage = new Dictionary<string, object>
		{
			{ "creditorAmount", ValidMessages.PartnerBankEarmarkFundsV1.CdtrAmt },
			{ "debtorAgentAccountIban", ValidMessages.PartnerBankEarmarkFundsV1.DbtrAgntAcct?.Id?.IBAN },
			{ "debtorAccountIban", ValidMessages.PartnerBankEarmarkFundsV1.DbtrAcct?.Id?.IBAN }
		};

		signDocumentRequest.Should().BeEquivalentTo(new VerifyRequest
		{
			RtgsGlobalId = SubscriberActions.DefaultSigningHeaders["from-rtgs-global-id"],
			PrivateSignature = SubscriberActions.DefaultSigningHeaders["pairwise-did-signature"],
			Alias = SubscriberActions.DefaultSigningHeaders["alias"],
		}, options => options.Excluding(x => x.Message));

		signDocumentRequest!.Message.Should().BeEquivalentTo(
			JsonSerializer.SerializeToElement(expectedMessage),
			options => options.ComparingByMembers<JsonElement>());
	}
}
