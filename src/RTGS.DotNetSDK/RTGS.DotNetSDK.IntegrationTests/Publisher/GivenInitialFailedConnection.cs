namespace RTGS.DotNetSDK.IntegrationTests.Publisher;

public class GivenInitialFailedConnection : IDisposable, IClassFixture<GrpcServerFixture>
{
	private const string BankPartnerDid = "bank-partner-did";
	private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

	private readonly GrpcServerFixture _grpcServer;

	private IRtgsPublisher _rtgsPublisher;
	private ToRtgsMessageHandler _toRtgsMessageHandler;
	private IHost _clientHost;

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
					TestData.ValidMessages.RtgsGlobalId,
					_grpcServer.ServerUri,
					new Uri("http://id-crypt-cloud-agent-api.com"),
					"id-crypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices(services => services.AddRtgsPublisher(rtgsSdkOptions))
				.Build();

			_rtgsPublisher = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
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
			.Awaiting(() => _rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequestV1(), BankPartnerDid))
			.Should()
			.ThrowAsync<Exception>();
	}

	[Fact]
	public async Task WhenSendingBigMessage_ThenThrowException()
	{
		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		receiver.ThrowOnConnection = true;

		await FluentActions
			.Awaiting(() => _rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequestV1 { EndToEndId = new string('e', 100_000) }, BankPartnerDid))
			.Should()
			.ThrowAsync<Exception>();
	}

	[Fact]
	public async Task WhenSubsequentConnectionCanBeOpened_ThenCanSendSubsequentMessagesToRtgs()
	{
		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		receiver.ThrowOnConnection = true;

		await FluentActions
			.Awaiting(() => _rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequestV1(), BankPartnerDid))
			.Should()
			.ThrowAsync<Exception>();

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		receiver.ThrowOnConnection = false;

		var result = await _rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequestV1(), BankPartnerDid);

		result.Should().Be(SendResult.Success);
	}
}
