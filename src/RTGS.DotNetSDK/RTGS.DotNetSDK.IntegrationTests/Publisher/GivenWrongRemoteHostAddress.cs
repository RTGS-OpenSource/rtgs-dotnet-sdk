namespace RTGS.DotNetSDK.IntegrationTests.Publisher;

public class GivenWrongRemoteHostAddress
{
	[Fact]
	public async Task WhenSending_ThenRpcExceptionThrown()
	{
		var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
				TestData.ValidMessages.RtgsGlobalId,
				new Uri("https://localhost:4567"),
				new Uri("https://id-crypt-service"))
			.EnableMessageSigning()
			.Build();

		using var clientHost = Host.CreateDefaultBuilder()
			.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
			.ConfigureServices(services => services.AddRtgsPublisher(rtgsSdkOptions))
			.UseSerilog()
			.Build();

		var rtgsPublisher = clientHost.Services.GetRequiredService<IRtgsPublisher>();

		await FluentActions.Awaiting(() => rtgsPublisher.SendBankPartnersRequestAsync(new BankPartnersRequestV1()))
			.Should().ThrowAsync<RpcException>();
	}
}
