using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.RtgsConnectionBrokerTests;

public class GivenWrongRemoteHostAddress
{
	[Fact]
	public async Task WhenSending_ThenRpcExceptionThrown()
	{
		var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
				TestData.ValidMessages.BankDid,
				new Uri("https://localhost:4567"),
				new Uri("http://id-crypt-cloud-agent-api.com"),
				"id-crypt-api-key",
				new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
			.Build();

		var idCryptMessageHandler = new StatusCodeHttpHandler(IdCryptEndPoints.MockHttpResponses);

		using var clientHost = Host.CreateDefaultBuilder()
			.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
			.ConfigureServices(services => services
				.AddRtgsPublisher(rtgsSdkOptions)
				.AddTestIdCryptHttpClient(idCryptMessageHandler))
			.UseSerilog()
			.Build();

		var rtgsConnectionBroker = clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();

		await FluentActions.Awaiting(() => rtgsConnectionBroker.SendInvitationAsync())
			.Should().ThrowAsync<RpcException>();
	}
}
