using System.Net;
using System.Net.Http;
using RTGS.DotNetSDK.Publisher.IntegrationTests.Extensions;
using RTGS.DotNetSDK.Publisher.IntegrationTests.HttpHandlers;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.RtgsConnectionBrokerTests;

public class GivenWrongRemoteHostAddress
{
	[Fact]
	public async Task WhenSending_ThenRpcExceptionThrown()
	{
		var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(
				ValidMessages.BankDid,
				new Uri("https://localhost:4567"),
				new Uri("http://id-crypt-cloud-agent-api.com"),
				"id-crypt-api-key",
				new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
			.Build();

		var idCryptMessageHandler = new StatusCodeHttpHandler(
			HttpStatusCode.OK,
			new StringContent(IdCryptTestMessages.ConnectionInviteResponseJson));

		using var clientHost = Host.CreateDefaultBuilder()
			.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
			.ConfigureServices(services => services
				.AddRtgsPublisher(rtgsPublisherOptions)
				.AddTestIdCryptHttpClient(idCryptMessageHandler))
			.UseSerilog()
			.Build();

		var rtgsConnectionBroker = clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();

		await FluentActions.Awaiting(() => rtgsConnectionBroker.SendInvitationAsync())
			.Should().ThrowAsync<RpcException>();
	}
}
