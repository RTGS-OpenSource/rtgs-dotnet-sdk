using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.RtgsConnectionBrokerTests;

public class GivenInitialFailedConnection : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

	private readonly GrpcServerFixture _grpcServer;
	private ToRtgsMessageHandler _toRtgsMessageHandler;
	private IHost _clientHost;
	private IRtgsConnectionBroker _rtgsConnectionBroker;

	public GivenInitialFailedConnection(GrpcServerFixture grpcServer)
	{
		_grpcServer = grpcServer;

		SetupDependencies();
	}

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
				.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
				.Build();

			var idCryptMessageHandler = StatusCodeHttpHandlerBuilder
				.Create()
				.WithOkResponse(CreateInvitation.HttpRequestResponseContext)
				.WithOkResponse(CreateInvitation.HttpRequestResponseContext)
				.WithOkResponse(GetPublicDid.HttpRequestResponseContext)
				.WithOkResponse(GetPublicDid.HttpRequestResponseContext)
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices(services => services
					.AddRtgsPublisher(rtgsSdkOptions)
					.AddTestIdCryptHttpClient(idCryptMessageHandler))
				.Build();

			_rtgsConnectionBroker = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
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
	public async Task WhenSending_ThenThrowException()
	{
		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		receiver.ThrowOnConnection = true;

		await FluentActions
			.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync())
			.Should()
			.ThrowAsync<Exception>();
	}

	[Fact]
	public async Task WhenSubsequentConnectionCanBeOpened_ThenCanSendSubsequentMessagesToRtgs()
	{
		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		receiver.ThrowOnConnection = true;

		await FluentActions
			.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync())
			.Should()
			.ThrowAsync<Exception>();

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		receiver.ThrowOnConnection = false;

		var result = await _rtgsConnectionBroker.SendInvitationAsync();

		result.SendResult.Should().Be(SendResult.Success);
	}
}
