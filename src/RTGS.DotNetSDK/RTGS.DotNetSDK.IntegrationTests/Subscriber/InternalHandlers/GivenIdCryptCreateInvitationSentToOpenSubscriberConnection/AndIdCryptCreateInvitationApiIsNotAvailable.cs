using Microsoft.AspNetCore.WebUtilities;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;
using RTGS.DotNetSDK.Subscriber.Handlers;
using ValidMessages = RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData.ValidMessages;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalHandlers.GivenIdCryptCreateInvitationSentToOpenSubscriberConnection;

public class AndIdCryptCreateInvitationApiIsNotAvailable : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(1_000);

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;
	private readonly List<IHandler> _allTestHandlers = new AllTestHandlers().ToList();

	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;
	private StatusCodeHttpHandler _idCryptMessageHandler;
	private AllTestHandlers.TestIdCryptCreateInvitationNotificationV1 _invitationNotificationHandler;

	public AndIdCryptCreateInvitationApiIsNotAvailable(GrpcServerFixture grpcServer)
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
				.WithOkResponse(GetPublicDid.HttpRequestResponseContext)
				.WithServiceUnavailableResponse(CreateInvitation.Path)
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
			var toRtgsMessageHandler = _grpcServer.Services.GetRequiredService<ToRtgsMessageHandler>();
			toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

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
	public async Task ThenInvitationNotSent()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		receiver.Connections.Should().BeEmpty();
	}

	[Fact]
	public async Task ThenLog()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		var inviteRequestQueryParams = QueryHelpers.ParseQuery(_idCryptMessageHandler.Requests[CreateInvitation.Path].Single().RequestUri!.Query);
		var alias = inviteRequestQueryParams["alias"];

		using var _ = new AssertionScope();
		var debugLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Debug);
		debugLogs.Select(log => log.Message)
			.Should().ContainSingle(msg => msg == $"Sending CreateInvitation request with alias {alias} to ID Crypt Cloud Agent");

		var errorLogs = _serilogContext.LogsFor("RTGS.DotNetSDK.Subscriber.Handlers.Internal.IdCryptCreateInvitationRequestV1Handler", LogEventLevel.Error);
		errorLogs.Select(log => log.Message)
			.Should().ContainSingle(msg => msg == $"Error occurred when sending CreateInvitation request with alias {alias} to ID Crypt Cloud Agent");
	}

	[Fact]
	public async Task ThenUserHandlerIsNotInvoked()
	{
		await _rtgsSubscriber.StartAsync(_allTestHandlers);

		await _fromRtgsSender.SendAsync("idcrypt.createinvitation.v1", ValidMessages.IdCryptCreateInvitationRequestV1);

		_invitationNotificationHandler.WaitForMessage(WaitForReceivedMessageDuration);

		_invitationNotificationHandler.ReceivedMessage.Should().BeNull();
	}
}
