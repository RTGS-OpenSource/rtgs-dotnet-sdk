namespace RTGS.DotNetSDK.Publisher.IntegrationTests;

public class GivenInitialFailedConnection : IAsyncLifetime, IClassFixture<GrpcServerFixture>
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

			_rtgsPublisher = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
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

	public async Task DisposeAsync()
	{
		if (_rtgsPublisher is not null)
		{
			await _rtgsPublisher.DisposeAsync();
		}

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
