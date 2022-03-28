﻿using IDCryptGlobal.Cloud.Agent.Identity;
using IDCryptGlobal.Cloud.Agent.Identity.Connection;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.IdCrypt;
using RTGS.DotNetSDK.IdCrypt.Messages;
using RTGS.DotNetSDK.Subscriber.Exceptions;

namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;

internal class IdCryptCreateInvitationRequestV1Handler : IIdCryptCreateInvitationRequestV1Handler
{
	private readonly ILogger<IdCryptCreateInvitationRequestV1Handler> _logger;
	private readonly IIdentityClient _identityClient;
	private readonly IIdCryptPublisher _idCryptPublisher;
	private IHandler<IdCryptCreateInvitationNotificationV1> _userHandler;

	public IdCryptCreateInvitationRequestV1Handler(
		ILogger<IdCryptCreateInvitationRequestV1Handler> logger,
		IIdentityClient identityClient,
		IIdCryptPublisher idCryptPublisher)
	{
		_logger = logger;
		_identityClient = identityClient;
		_idCryptPublisher = idCryptPublisher;
	}

	public void SetUserHandler(IHandler<IdCryptCreateInvitationNotificationV1> userHandler) =>
		_userHandler = userHandler;

	public async Task HandleMessageAsync(IdCryptCreateInvitationRequestV1 createInvitationRequest)
	{
		if (_userHandler is null)
		{
			throw new RtgsSubscriberException(
				$"User Handler not set in {nameof(IdCryptCreateInvitationRequestV1Handler)} when invoking {nameof(HandleMessageAsync)}(). " +
				$"Ensure {nameof(SetUserHandler)}() has been called first.");
		}

		var alias = Guid.NewGuid().ToString();

		var invitation = await CreateIdCryptInvitationAsync(alias);
		var agentPublicDid = await GetIdCryptAgentPublicDidAsync();
		var bankPartnerDid = createInvitationRequest.BankPartnerDid;

		await SendInvitationToBankAsync(alias, invitation.Invitation, agentPublicDid, bankPartnerDid, default);

		var invitationNotification = new IdCryptCreateInvitationNotificationV1
		{
			Alias = alias,
			ConnectionId = invitation.ConnectionID,
			BankPartnerDid = bankPartnerDid
		};

		await _userHandler.HandleMessageAsync(invitationNotification);
	}

	private async Task<ConnectionInviteResponseModel> CreateIdCryptInvitationAsync(string alias)
	{
		const bool autoAccept = true;
		const bool multiUse = false;
		const bool usePublicDid = false;

		try
		{
			_logger.LogDebug("Sending CreateInvitation request with alias {Alias} to ID Crypt Cloud Agent", alias);

			var response = await _identityClient.Connection.CreateInvitation(
				alias,
				autoAccept,
				multiUse,
				usePublicDid);

			_logger.LogDebug("Sent CreateInvitation request with alias {Alias} to ID Crypt Cloud Agent", alias);

			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when sending CreateInvitation request with alias {Alias} to ID Crypt Cloud Agent", alias);

			throw;
		}
	}

	private async Task<string> GetIdCryptAgentPublicDidAsync()
	{
		try
		{
			_logger.LogDebug("Sending GetPublicDid request to ID Crypt Cloud Agent");

			var response = await _identityClient.Vault.GetPublicDID();

			_logger.LogDebug("Sent GetPublicDid request to ID Crypt Cloud Agent");

			return response.Result.DID;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when sending GetPublicDid request to ID Crypt Cloud Agent");

			throw;
		}
	}

	private async Task SendInvitationToBankAsync(
		string alias,
		ConnectionInvitation invitation,
		string agentPublicDid,
		string bankPartnerDid,
		CancellationToken cancellationToken)
	{
		_logger.LogDebug("Sending Invitation with alias {Alias} to Bank '{BankPartnerDid}'", alias, bankPartnerDid);

		var invitationMessage = new IdCryptInvitationV1
		{
			Alias = alias,
			Id = invitation.ID,
			Label = invitation.Label,
			RecipientKeys = invitation.RecipientKeys,
			ServiceEndPoint = invitation.ServiceEndPoint,
			Type = invitation.Type,
			AgentPublicDid = agentPublicDid
		};

		SendResult sendResult;
		try
		{
			sendResult = await _idCryptPublisher
				.SendIdCryptInvitationToBankAsync(invitationMessage, bankPartnerDid, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception occurred when sending IdCrypt Invitation with alias {Alias} to Bank '{BankPartnerDid}'", alias, bankPartnerDid);
			throw;
		}

		if (sendResult is not SendResult.Success)
		{
			_logger.LogError("Error occurred when sending IdCrypt Invitation with alias {Alias} to Bank '{BankPartnerDid}'", alias, bankPartnerDid);

			throw new RtgsSubscriberException(
				$"Error occurred when sending IdCrypt Invitation with alias {alias} to Bank '{bankPartnerDid}'");
		}

		_logger.LogDebug("Sent Invitation with alias {Alias} to Bank '{BankPartnerDid}'", alias, bankPartnerDid);
	}
}
