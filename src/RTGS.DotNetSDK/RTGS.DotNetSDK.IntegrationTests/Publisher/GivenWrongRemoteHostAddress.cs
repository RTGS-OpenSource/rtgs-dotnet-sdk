namespace RTGS.DotNetSDK.IntegrationTests.Publisher;

public class GivenWrongRemoteHostAddress
{
	[Fact]
	public async Task WhenSending_ThenRpcExceptionThrown()
	{
		var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
				TestData.ValidMessages.RtgsGlobalId,
				new Uri("https://localhost:4567"),
				new Uri("http://id-crypt-cloud-agent-api.com"),
				"id-crypt-api-key",
				new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
			.Build();

		using var clientHost = Host.CreateDefaultBuilder()
			.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
			.ConfigureServices(services => services.AddRtgsPublisher(rtgsSdkOptions))
			.UseSerilog()
			.Build();

		var rtgsPublisher = clientHost.Services.GetRequiredService<IRtgsPublisher>();

		await FluentActions.Awaiting(() => rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequestV1(), "bank-partner-rtgs-global-id"))
			.Should().ThrowAsync<RpcException>();
	}
}
