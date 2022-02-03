namespace RTGS.DotNetSDK.Publisher.IntegrationTests;

public class GivenMultipleOpenConnections : IAsyncLifetime, IClassFixture<GrpcServerFixture>
{
	private const string BankPartnerDid = "bank-partner-did";
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
			var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(
					ValidMessages.BankDid,
					_grpcServer.ServerUri,
					"",
					new Uri("http://example.com"),
					"")
				.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices(services => services.AddRtgsPublisher(rtgsPublisherOptions))
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
		const int PublisherCount = 1;

		await using var rtgsPublisher1 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
		await using var rtgsPublisher2 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
		await using var rtgsPublisher3 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
		await using var rtgsPublisher4 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
		await using var rtgsPublisher5 = _clientHost.Services.GetRequiredService<IRtgsPublisher>();

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsPublisher1.SendAtomicLockRequestAsync(new AtomicLockRequestV1(), BankPartnerDid);

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsPublisher2.SendAtomicLockRequestAsync(new AtomicLockRequestV1(), BankPartnerDid);

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsPublisher3.SendAtomicLockRequestAsync(new AtomicLockRequestV1(), BankPartnerDid);

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsPublisher4.SendAtomicLockRequestAsync(new AtomicLockRequestV1(), BankPartnerDid);

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());
		await rtgsPublisher5.SendAtomicLockRequestAsync(new AtomicLockRequestV1(), BankPartnerDid);

		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		using var _ = new AssertionScope();
		receiver.Connections.Count.Should().Be(PublisherCount);
		receiver.Connections.SelectMany(connection => connection.Requests).Count().Should().Be(5);
	}
}
