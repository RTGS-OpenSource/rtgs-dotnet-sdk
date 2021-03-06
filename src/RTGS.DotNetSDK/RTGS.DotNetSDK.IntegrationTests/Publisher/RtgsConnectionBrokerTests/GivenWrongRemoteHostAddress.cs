using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.RtgsConnectionBrokerTests;

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

		var idCryptServiceHttpHandler = StatusCodeHttpHandlerBuilderFactory
			.Create()
			.WithOkResponse(CreateConnectionForRtgs.HttpRequestResponseContext)
			.Build();

		using var clientHost = Host.CreateDefaultBuilder()
			.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
			.ConfigureServices(services => services
				.AddRtgsPublisher(rtgsSdkOptions)
				.AddTestIdCryptServiceHttpClient(idCryptServiceHttpHandler))
			.UseSerilog()
			.Build();

		var rtgsConnectionBroker = clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();

		await FluentActions.Awaiting(() => rtgsConnectionBroker.SendInvitationAsync())
			.Should().ThrowAsync<RpcException>();
	}
}
