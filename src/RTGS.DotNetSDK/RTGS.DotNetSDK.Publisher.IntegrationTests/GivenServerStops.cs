using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.DotNetSDK.Publisher.Extensions;
using RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;
using RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer;
using RTGS.DotNetSDK.Publisher.Messages;
using Xunit;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class GivenServerStops
	{
		[Fact]
		public async Task WhenSendingMessage_ThenRpcExceptionOrIOExceptionThrown()
		{
			using var server = new GrpcTestServer();
			var serverUri = await server.StartAsync();

			var toRtgsMessageHandler = server.Services.GetRequiredService<ToRtgsMessageHandler>();
			toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			var rtgsClientOptions = RtgsClientOptions.Builder.CreateNew()
				.BankDid(ValidRequests.BankDid)
				.RemoteHost(serverUri.ToString())
				.Build();

			using var clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices((_, services) => services.AddRtgsPublisher(rtgsClientOptions))
				.Build();

			await using var rtgsPublisher = clientHost.Services.GetRequiredService<IRtgsPublisher>();

			var result = await rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequest());
			result.Should().Be(SendResult.Success);

			await server.StopAsync();
			server.Dispose();

			var exceptionAssertions = await FluentActions.Awaiting(() => rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequest()))
				.Should().ThrowAsync<Exception>();

			// One of two exceptions can be thrown depending on how far along the call is.
			exceptionAssertions.And.GetType()
				.Should().Match(exceptionType => exceptionType == typeof(RpcException)
												 || exceptionType == typeof(IOException));
		}
	}
}
