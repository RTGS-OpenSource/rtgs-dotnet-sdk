using System.Net;
using RTGS.DotNetSDK.Publisher.IntegrationTests.Extensions;
using RTGS.DotNetSDK.Publisher.IntegrationTests.HttpHandlers;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.RtgsConnectionBrokerTests;

public class GivenMultipleOpenConnections : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

	private readonly GrpcServerFixture _grpcServer;
	private ToRtgsMessageHandler _toRtgsMessageHandler;
	private IHost _clientHost;

	public GivenMultipleOpenConnections(GrpcServerFixture grpcServer)
	{
		_grpcServer = grpcServer;

		SetupDependencies();
	}

	private void SetupDependencies()
	{
		try
		{
			var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(
					ValidMessages.BankDid,
					_grpcServer.ServerUri,
					new Uri("http://id-crypt-cloud-agent-api.com"),
					"id-crypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
				.Build();

			var idCryptMessageHandler = new StatusCodeHttpHandler(HttpStatusCode.OK, IdCryptEndPoints.HttpContentsMock);

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices(services => services
					.AddRtgsPublisher(rtgsPublisherOptions)
					.AddTestIdCryptHttpClient(idCryptMessageHandler))
				.Build();

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
	public async Task WhenSendingSequentially_ThenCanSendToRtgs()
	{
		const int PublisherCount = 1;

		var rtgsConnectionBroker1 = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
		var rtgsConnectionBroker2 = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
		var rtgsConnectionBroker3 = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
		var rtgsConnectionBroker4 = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
		var rtgsConnectionBroker5 = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsConnectionBroker1.SendInvitationAsync();

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsConnectionBroker2.SendInvitationAsync();

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsConnectionBroker3.SendInvitationAsync();

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsConnectionBroker4.SendInvitationAsync();

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsConnectionBroker5.SendInvitationAsync();

		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		using var _ = new AssertionScope();
		receiver.Connections.Count.Should().Be(PublisherCount);
		receiver.Connections.SelectMany(connection => connection.Requests).Count().Should().Be(5);
	}
}
