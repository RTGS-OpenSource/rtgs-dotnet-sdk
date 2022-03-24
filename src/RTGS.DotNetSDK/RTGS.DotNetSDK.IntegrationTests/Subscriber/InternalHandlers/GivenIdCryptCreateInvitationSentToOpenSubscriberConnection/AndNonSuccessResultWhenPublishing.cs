using Microsoft.AspNetCore.WebUtilities;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;
using RTGS.DotNetSDK.Subscriber.Handlers;
using ValidMessages = RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData.ValidMessages;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalHandlers.GivenIdCryptCreateInvitationSentToOpenSubscriberConnection;

public class AndNonSuccessResultWhenPublishing : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(1_000);

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;
	private readonly List<IHandler> _allTestHandlers = new AllTestHandlers().ToList();

	private AllTestHandlers.TestIdCryptCreateInvitationNotificationV1 _invitationNotificationHandler;
	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;
	private StatusCodeHttpHandler _idCryptMessageHandler;

	public AndNonSuccessResultWhenPublishing(GrpcServerFixture grpcServer)
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
					ValidMessages.BankDid,
					_grpcServer.ServerUri,
					new Uri("http://id-crypt-cloud-agent-api.com"),
					"id-crypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.Build();

			_idCryptMessageHandler = StatusCodeHttpHandlerBuilder
				.Create()
				.WithOkResponse(CreateInvitation.HttpRequestResponseContext)
				.WithOkResponse(GetPublicDid.HttpRequestResponseContext)
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices(services => services
					.AddRtgsSubscriber(rtgsSdkOptions)
					.AddTestIdCryptHttpClient(_idCryptMessageHandler))
				.UseSerilog()
				.Build();

			_fromRtgsSender = _grpcServer.Services.GetRequiredService<FromRtgsSender>();
			_rtgsSubscriber = _clientHost.Services.GetRequiredService<IRtgsSubscriber>();
			var toRtgsMessageHandler = _grpcServer.Services.GetRequiredService<ToRtgsMessageHandler>();
			toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithFailure());

			_invitationNotificationHandler = _allTestHandlers.OfType<AllTestHandlers.TestIdCryptCreateInvitationNotificationV1>().Single();
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
	public async Task ThenLog()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		var inviteRequestQueryParams = QueryHelpers.ParseQuery(
			_idCryptMessageHandler.Requests[CreateInvitation.Path].Single().RequestUri!.Query);
		var alias = inviteRequestQueryParams["alias"];

		var expectedDebugLogs = new List<LogEntry>
		{
			new ($"Sending CreateInvitation request with alias {alias} to ID Crypt Cloud Agent", LogEventLevel.Debug),
			new ($"Sent CreateInvitation request with alias {alias} to ID Crypt Cloud Agent", LogEventLevel.Debug),
			new ("Sending GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug),
			new ("Sent GetPublicDid request to ID Crypt Cloud Agent", LogEventLevel.Debug),
			new ($"Sending Invitation with alias {alias} to Bank '{ValidMessages.IdCryptCreateInvitationRequestV1.BankPartnerDid}'", LogEventLevel.Debug),
		};

		using var _ = new AssertionScope();

		_serilogContext
			.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Debug)
			.Should().BeEquivalentTo(expectedDebugLogs, options => options.WithStrictOrdering());

		_serilogContext
			.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Error)
			.Should().ContainSingle()
			.Which.Message.Should().Be($"Error occurred when sending IdCrypt Invitation with alias {alias} to Bank '{ValidMessages.IdCryptCreateInvitationRequestV1.BankPartnerDid}'");
	}
}
