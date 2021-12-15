namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class GivenWrongRemoteHostAddress
	{
		[Fact]
		public async Task WhenSending_ThenRpcExceptionThrown()
		{
			var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(ValidMessages.BankDid, new Uri("https://localhost:4567"))
				.Build();

			using var clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices(services => services.AddRtgsPublisher(rtgsPublisherOptions))
				.UseSerilog()
				.Build();

			await using var rtgsPublisher = clientHost.Services.GetRequiredService<IRtgsPublisher>();

			await FluentActions.Awaiting(() => rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequestV1()))
				.Should().ThrowAsync<RpcException>();
		}
	}
}
