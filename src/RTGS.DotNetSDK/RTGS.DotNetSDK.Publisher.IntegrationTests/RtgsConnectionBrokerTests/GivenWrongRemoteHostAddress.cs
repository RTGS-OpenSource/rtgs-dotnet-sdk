using System.Net;
using System.Net.Http;
using IDCryptGlobal.Cloud.Agent.Identity;
using Microsoft.Extensions.Options;
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
				new Uri("http://id-crypt-cloud-agent.com"),
				Guid.NewGuid().ToString(), "http://id-crypt-cloud-agent.com")
			.Build();

		var idCryptMessageHandler = new StatusCodeHttpHandler(
			HttpStatusCode.OK,
			new StringContent(IdCryptTestMessages.ConnectionInviteResponseJson));

		using var clientHost = Host.CreateDefaultBuilder()
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
			.UseSerilog()
			.Build();

		var rtgsConnectionBroker = clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();

		await FluentActions.Awaiting(() => rtgsConnectionBroker.SendInvitationAsync())
			.Should().ThrowAsync<RpcException>();
	}
}
