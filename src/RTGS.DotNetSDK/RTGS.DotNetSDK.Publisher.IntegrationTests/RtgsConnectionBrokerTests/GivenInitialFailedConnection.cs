﻿using System.Net;
using System.Net.Http;
using IDCryptGlobal.Cloud.Agent.Identity;
using IDCryptGlobal.Cloud.Agent.Identity.Connection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RTGS.DotNetSDK.Publisher.IntegrationTests.HttpHandlers;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.RtgsConnectionBrokerTests;

public class GivenInitialFailedConnection : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

	private readonly GrpcServerFixture _grpcServer;
	private readonly ConnectionInviteResponseModel _connectionInviteResponse;
	private ToRtgsMessageHandler _toRtgsMessageHandler;
	private IHost _clientHost;
	private IRtgsConnectionBroker _rtgsConnectionBroker;

	public GivenInitialFailedConnection(GrpcServerFixture grpcServer)
	{
		_grpcServer = grpcServer;

		_connectionInviteResponse = new ConnectionInviteResponseModel
		{
			ConnectionID = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
			Invitation = new ConnectionInvitation
			{
				ID = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
				Type = "https://didcomm.org/my-family/1.0/my-message-type",
				Label = "Bob",
				RecipientKeys = new[]
				   {
						"H3C2AVvLMv6gmMNam3uVAjZpfkcJCwDwnZn6z3wXmqPV"
					},
				ServiceEndPoint = "http://192.168.56.101:8020"
			}
		};

		SetupDependencies();
	}

	private void SetupDependencies()
	{
		try
		{
			var rtgsPublisherOptions = RtgsPublisherOptions.Builder.CreateNew(
					ValidMessages.BankDid, 
					_grpcServer.ServerUri,
					Guid.NewGuid().ToString(),
					new Uri("http://example.com"),
					"http://example.com")
				.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
				.Build();

			var connectionInviteResponseJson = JsonConvert.SerializeObject(_connectionInviteResponse);

			var idCryptMessageHandler = new StatusCodeHttpHandler(HttpStatusCode.OK, new StringContent(connectionInviteResponseJson));

			_clientHost = Host.CreateDefaultBuilder()
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
				.Build();

			_rtgsConnectionBroker = _clientHost.Services.GetRequiredService<IRtgsConnectionBroker>();
			_toRtgsMessageHandler = _grpcServer.Services.GetRequiredService<ToRtgsMessageHandler>();
		}
		catch (Exception)
		{
			// If an exception occurs then manually clean up as IAsyncLifetime.DisposeAsync is not called.
			// See https://github.com/xunit/xunit/discussions/2313 for further details.
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
			.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync())
			.Should()
			.ThrowAsync<Exception>();
	}

	[Fact]
	public async Task WhenSendingBigMessage_ThenThrowException()
	{
		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		receiver.ThrowOnConnection = true;

		await FluentActions
			.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync())
			.Should()
			.ThrowAsync<Exception>();
	}

	[Fact]
	public async Task WhenSubsequentConnectionCanBeOpened_ThenCanSendSubsequentMessagesToRtgs()
	{
		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		receiver.ThrowOnConnection = true;

		await FluentActions
			.Awaiting(() => _rtgsConnectionBroker.SendInvitationAsync())
			.Should()
			.ThrowAsync<Exception>();

		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		receiver.ThrowOnConnection = false;

		var result = await _rtgsConnectionBroker.SendInvitationAsync();

		result.SendResult.Should().Be(SendResult.Success);
	}
}
