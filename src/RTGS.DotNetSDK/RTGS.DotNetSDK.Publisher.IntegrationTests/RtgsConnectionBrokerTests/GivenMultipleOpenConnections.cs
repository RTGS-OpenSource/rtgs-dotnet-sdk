using System.Net;
using System.Net.Http;
using IDCryptGlobal.Cloud.Agent.Identity;
using Microsoft.Extensions.Options;
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
                    Guid.NewGuid().ToString(),
                    new Uri("http://example.com"),
                    "http://example.com")
                .WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
                .Build();

            var idCryptMessageHandler = new StatusCodeHttpHandler(
				HttpStatusCode.OK, 
				new StringContent(IdCryptTestMessages.ConnectionInviteResponseJson));

            _clientHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
                .ConfigureServices(services => services
                    .AddRtgsPublisher(rtgsPublisherOptions)
                    .AddSingleton(idCryptMessageHandler)
                    .AddHttpClient<IIdentityClient, IdentityClient>((httpClient, serviceProvider) =>
                    {
                        var identityOptions = serviceProvider.GetRequiredService<IOptions<IdentityConfig>>();
                        var identityClient = new IdentityClient(httpClient, identityOptions);

                        return identityClient;
                    })
                    .AddHttpMessageHandler<StatusCodeHttpHandler>())
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
	public void WhenRequestingMultipleConnectionBrokers_ThenSameConnectionBrokerIsReturned()
	{
		var connectionBroker1 = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
		var connectionBroker2 = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();

		connectionBroker1.Should().BeSameAs(connectionBroker2);
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
