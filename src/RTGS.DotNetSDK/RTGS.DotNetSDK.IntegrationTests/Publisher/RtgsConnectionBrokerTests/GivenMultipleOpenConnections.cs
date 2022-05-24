using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.RtgsConnectionBrokerTests;

public sealed class GivenMultipleOpenConnections : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

	private readonly GrpcServerFixture _grpcServer;
	private QueueableStatusCodeHttpHandler _idCryptServiceHttpHandler;
	private IHost _clientHost;
	private ToRtgsMessageHandler _toRtgsMessageHandler;

	public GivenMultipleOpenConnections(GrpcServerFixture grpcServer)
	{
		_grpcServer = grpcServer;

		SetupDependencies();
	}

	private void SetupDependencies()
	{
		try
		{
			var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
					TestData.ValidMessages.RtgsGlobalId,
					_grpcServer.ServerUri,
					new Uri("https://id-crypt-service"))
				.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
				.Build();

			_idCryptServiceHttpHandler = StatusCodeHttpHandlerBuilderFactory
				.CreateQueueable()
				.WithOkResponse(CreateConnection.HttpRequestResponseContext)
				.WithOkResponse(CreateConnection.HttpRequestResponseContext)
				.WithOkResponse(CreateConnection.HttpRequestResponseContext)
				.WithOkResponse(CreateConnection.HttpRequestResponseContext)
				.WithOkResponse(CreateConnection.HttpRequestResponseContext)
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices(services => services
					.AddRtgsPublisher(rtgsSdkOptions)
					.AddTestIdCryptServiceHttpClient(_idCryptServiceHttpHandler))
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
		const int publisherCount = 1;

		var rtgsConnectionBroker1 = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
		var rtgsConnectionBroker2 = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
		var rtgsConnectionBroker3 = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
		var rtgsConnectionBroker4 = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
		var rtgsConnectionBroker5 = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsConnectionBroker1.SendInvitationAsync("rtgs-global-id");

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsConnectionBroker2.SendInvitationAsync("rtgs-global-id");

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsConnectionBroker3.SendInvitationAsync("rtgs-global-id");

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsConnectionBroker4.SendInvitationAsync("rtgs-global-id");

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsConnectionBroker5.SendInvitationAsync("rtgs-global-id");

		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		using var _ = new AssertionScope();
		receiver.Connections.Count.Should().Be(publisherCount);
		receiver.Connections.SelectMany(connection => connection.Requests).Count().Should().Be(5);
	}
}
