﻿using IDCryptGlobal.Cloud.Agent.Identity;
using IDCryptGlobal.Cloud.Agent.Identity.Connection;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Extensions;
using RTGS.DotNetSDK.IdCrypt;
using RTGS.DotNetSDK.IdCrypt.Messages;
using RTGS.DotNetSDK.Subscriber.Exceptions;

namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;

internal class IdCryptBankInvitationV1Handler : IIdCryptBankInvitationV1Handler
{
	private readonly ILogger<IdCryptBankInvitationV1Handler> _logger;
	private readonly IIdentityClient _identityClient;
	private readonly IIdCryptPublisher _idCryptPublisher;
	private IHandler<IdCryptBankInvitationNotificationV1> _userHandler;

	public IdCryptBankInvitationV1Handler(
		ILogger<IdCryptBankInvitationV1Handler> logger,
		IIdentityClient identityClient,
		IIdCryptPublisher idCryptPublisher)
	{
		_logger = logger;
		_identityClient = identityClient;
		_idCryptPublisher = idCryptPublisher;
	}

	public void SetUserHandler(IHandler<IdCryptBankInvitationNotificationV1> userHandler) =>
		_userHandler = userHandler;

	public async Task HandleMessageAsync(IdCryptBankInvitationV1 bankInvitation)
	{
		if (_userHandler is null)
		{
			throw new RtgsSubscriberException(
				$"User Handler not set in {nameof(IdCryptBankInvitationV1Handler)} when invoking {nameof(HandleMessageAsync)}(). " +
				$"Ensure {nameof(SetUserHandler)}() has been called first.");
		}

		var connection = await AcceptInviteAsync(bankInvitation);


		// TODO JLIQ - Check connection state here
		//  if active, call _userHandler and _idCryptPublisher
		//  else trigger polling

		PollConnectionState(connection.ConnectionID, bankInvitation.FromBankDid);
	}

	private async Task<ConnectionAccepted> AcceptInviteAsync(IdCryptBankInvitationV1 bankInvitation)
	{
		var invitation = bankInvitation.Invitation;
		var connectionInvite = new ConnectionInvite
		{
			Alias = invitation.Alias,
			Label = invitation.Label,
			RecipientKeys = invitation.RecipientKeys.ToArray(),
			ID = invitation.Id,
			Type = invitation.Type,
			ServiceEndPoint = invitation.ServiceEndPoint
		};

		try
		{
			_logger.LogDebug("Sending ReceiveAcceptInvitation request to ID Crypt for invitation from bank '{BankDid}'",
				bankInvitation.FromBankDid);

			var response = await _identityClient.Connection.ReceiveAcceptInvitation(connectionInvite);

			_logger.LogDebug("ID Crypt invitation from bank '{BankDid}' accepted",
				bankInvitation.FromBankDid);

			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex,
				"Error occurred when sending ReceiveAcceptInvitation request with alias {Alias} to ID Crypt Cloud Agent",
				invitation.Alias);
			throw;
		}
	}

	private void PollConnectionState(string connectionId, string fromBankDid)
	{
		_logger.LogDebug("Polling for connection '{ConnectionId}' state for invitation from bank '{BankDid}'", connectionId, fromBankDid);

		Task.Run(async () =>
		{
			ConnectionAccepted connection;
			do
			{
				// _logger.LogDebug("");
				connection = await _identityClient.Connection.GetConnection(connectionId);
				// _logger.LogDebug("");
			} while (connection.State is not "active" and not "error");

			if (connection.State is "active")
			{
				// TODO JLIQ - Should the user handler be called first?
				//  if so this breaks the test as we use `WaitForMessage` on the user handler
				//  and the publisher hasn't been called at that point. How do we wait on the publisher?
				var invitationConfirmation = new IdCryptInvitationConfirmationV1 { Alias = connection.Alias };

				await _idCryptPublisher.SendIdCryptInvitationConfirmationAsync(invitationConfirmation, fromBankDid, CancellationToken.None);

				var invitationNotification = new IdCryptBankInvitationNotificationV1
				{
					BankPartnerDid = fromBankDid,
					Alias = connection.Alias,
					ConnectionId = connection.ConnectionID
				};

				await _userHandler.HandleMessageAsync(invitationNotification);
			}
		}).Forget(_logger);
	}
}
