namespace RTGS.DotNetSDK.IntegrationTests.Publisher;

public class GivenMultipleOpenConnections : IAsyncLifetime, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

	private readonly GrpcServerFixture _grpcServer;
	private ToRtgsMessageHandler _toRtgsMessageHandler;
	private IHost _clientHost;

	public GivenMultipleOpenConnections(GrpcServerFixture grpcServer)
	{
		_grpcServer = grpcServer;
	}

	public async Task InitializeAsync()
	{
		try
		{
			var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
					TestData.ValidMessages.RtgsGlobalId,
					_grpcServer.ServerUri,
					new Uri("https://id-crypt-service"))
				.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
				.EnableMessageSigning()
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices(services => services.AddRtgsPublisher(rtgsSdkOptions))
				.Build();

			_toRtgsMessageHandler = _grpcServer.Services.GetRequiredService<ToRtgsMessageHandler>();
		}
		catch (Exception)
		{
			// If an exception occurs then manually clean up as IAsyncLifetime.DisposeAsync is not called.
			// See https://github.com/xunit/xunit/discussions/2313 for further details.
			await DisposeAsync();

			throw;
		}
	}

	public Task DisposeAsync()
	{
		_clientHost?.Dispose();

		_grpcServer.Reset();

		return Task.CompletedTask;
	}

	[Fact]
	public void WhenRequestingMultiplePublishers_ThenSamePublisherIsReturned()
	{
		var rtgsPublisher1 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
		var rtgsPublisher2 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();

		rtgsPublisher1.Should().BeSameAs(rtgsPublisher2);
	}

	[Fact]
	public async Task WhenSendingSequentially_ThenCanSendToRtgs()
	{
		const int publisherCount = 1;

		var rtgsPublisher1 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
		var rtgsPublisher2 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
		var rtgsPublisher3 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
		var rtgsPublisher4 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
		var rtgsPublisher5 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsPublisher1.SendBankPartnersRequestAsync(new BankPartnersRequestV1());

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsPublisher2.SendBankPartnersRequestAsync(new BankPartnersRequestV1());

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsPublisher3.SendBankPartnersRequestAsync(new BankPartnersRequestV1());

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsPublisher4.SendBankPartnersRequestAsync(new BankPartnersRequestV1());

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsPublisher5.SendBankPartnersRequestAsync(new BankPartnersRequestV1());

		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		using var _ = new AssertionScope();
		receiver.Connections.Count.Should().Be(publisherCount);
		receiver.Connections.SelectMany(connection => connection.Requests).Count().Should().Be(5);
	}
}
