﻿using System.Net;
using System.Net.Http;
using IDCryptGlobal.Cloud.Agent.Identity;
using Microsoft.Extensions.Options;
using RTGS.DotNetSDK.Publisher.IntegrationTests.HttpHandlers;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.RtgsConnectionBrokerTests;

public class GivenInitialFailedConnection : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

	private readonly GrpcServerFixture _grpcServer;
	private ToRtgsMessageHandler _toRtgsMessageHandler;
	private IHost _clientHost;
	private IRtgsConnectionBroker _rtgsConnectionBroker;

	public GivenInitialFailedConnection(GrpcServerFixture grpcServer)
	{
		_grpcServer = grpcServer;
	
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

			var idCryptMessageHandler = new StatusCodeHttpHandler(
				HttpStatusCode.OK, 
				new StringContent(IdCryptTestMessages.ConnectionInviteResponseJson));

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
