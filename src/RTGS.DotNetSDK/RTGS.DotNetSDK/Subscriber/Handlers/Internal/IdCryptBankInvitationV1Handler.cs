using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.IdCrypt;
using RTGS.DotNetSDK.IdCrypt.Messages;
using RTGS.DotNetSDK.Subscriber.Exceptions;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;

internal class IdCryptBankInvitationV1Handler : IIdCryptBankInvitationV1Handler
{
	private readonly ILogger<IdCryptBankInvitationV1Handler> _logger;
	private readonly IConnectionsClient _connectionsClient;
	private readonly IWalletClient _walletClient;
	private readonly IIdCryptPublisher _idCryptPublisher;
	private IHandler<IdCryptBankInvitationNotificationV1> _userHandler;

	public IdCryptBankInvitationV1Handler(
		ILogger<IdCryptBankInvitationV1Handler> logger,
		IConnectionsClient connectionsClient,
		IWalletClient walletClient,
		IIdCryptPublisher idCryptPublisher)
	{
		_logger = logger;
		_connectionsClient = connectionsClient;
		_walletClient = walletClient;
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

		_ = WaitForActiveConnectionAndSendConfirmation(connection.ConnectionId, bankInvitation.FromBankDid);
	}

	private async Task<ConnectionResponse> AcceptInviteAsync(IdCryptBankInvitationV1 bankInvitation)
	{
		var invitation = bankInvitation.Invitation;
		var connectionInvite = new ReceiveAndAcceptInvitationRequest
		{
			Alias = invitation.Alias,
			Label = invitation.Label,
			RecipientKeys = invitation.RecipientKeys.ToArray(),
			Id = invitation.Id,
			Type = invitation.Type,
			ServiceEndPoint = invitation.ServiceEndPoint
		};

		try
		{
			_logger.LogDebug("Sending ReceiveAcceptInvitation request to ID Crypt for invitation from bank '{FromBankDid}'",
				bankInvitation.FromBankDid);

			var response = await _connectionsClient.ReceiveAndAcceptInvitationAsync(connectionInvite, default);

			_logger.LogDebug("Sent ReceiveAcceptInvitation request to ID Crypt for invitation from bank '{FromBankDid}'",
				bankInvitation.FromBankDid);

			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex,
				"Error occurred when sending ReceiveAcceptInvitation request to ID Crypt for invitation from bank '{FromBankDid}'",
				bankInvitation.FromBankDid);
			throw;
		}
	}

	private async Task WaitForActiveConnectionAndSendConfirmation(string connectionId, string fromBankDid)
	{
		_logger.LogDebug("Polling for connection '{ConnectionId}' state for invitation from bank '{FromBankDid}'", connectionId, fromBankDid);

		ConnectionResponse connection;
		try
		{
			connection = await PollConnectionState(connectionId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex,
				"Error occured when polling for connection '{ConnectionId}' state for invitation from bank '{FromBankDid}'",
				connectionId,
				fromBankDid);
			throw;
		}

		_logger.LogDebug("Finished polling for connection '{ConnectionId}' state for invitation from bank '{FromBankDid}'", connectionId, fromBankDid);

		await HandleInvitationConfirmation(fromBankDid, connection);
	}

	private async Task<ConnectionResponse> PollConnectionState(string connectionId)
	{
		var maxPollTime = TimeSpan.FromSeconds(30);
		var pollInterval = TimeSpan.FromSeconds(10);

		ConnectionResponse connection;

		var watch = Stopwatch.StartNew();
		while (true)
		{
			connection = await GetConnection(connectionId);

			if (connection.State is "active")
			{
				break;
			}

			if (watch.Elapsed > maxPollTime)
			{
				throw new RtgsSubscriberException("Timeout whilst waiting for ID Crypt invitation to be accepted");
			}

			await Task.Delay(pollInterval);
		}

		return connection;
	}

	private async Task<ConnectionResponse> GetConnection(string connectionId)
	{
		try
		{
			_logger.LogDebug("Sending GetConnection request to ID Crypt with connection Id '{ConnectionId}'",
				connectionId);

			var connection = new ConnectionResponse();
			throw new NotImplementedException();
			//var connection = await _connectionsClient.GetConnection(connectionId);

			_logger.LogDebug("Sent GetConnection request to ID Crypt with connection Id '{ConnectionId}'",
				connectionId);

			return connection;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex,
				"Error occurred when sending GetConnection request to ID Crypt with connection Id '{ConnectionId}'",
				connectionId);
			throw;
		}
	}

	private async Task HandleInvitationConfirmation(string fromBankDid, ConnectionResponse connection)
	{
		var agentPublicDid = await GetIdCryptAgentPublicDidAsync();

		await SendInvitationConfirmationAsync(connection.Alias, agentPublicDid, fromBankDid);

		await InvokeUserHandler(fromBankDid, connection);
	}

	private async Task<string> GetIdCryptAgentPublicDidAsync()
	{
		try
		{
			_logger.LogDebug("Sending GetPublicDid request to ID Crypt Cloud Agent");

			var publicDid = await _walletClient.GetPublicDidAsync(default);

			_logger.LogDebug("Sent GetPublicDid request to ID Crypt Cloud Agent");

			return publicDid;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when sending GetPublicDid request to ID Crypt Cloud Agent");

			throw;
		}
	}

	private async Task SendInvitationConfirmationAsync(string alias, string agentPublicDid, string fromBankDid)
	{
		_logger.LogDebug("Sending ID Crypt invitation confirmation to bank '{FromBankDid}'", fromBankDid);

		var invitationConfirmation = new IdCryptInvitationConfirmationV1
		{
			Alias = alias,
			AgentPublicDid = agentPublicDid
		};

		SendResult sendResult;
		try
		{
			sendResult = await _idCryptPublisher.SendIdCryptInvitationConfirmationAsync(
				invitationConfirmation,
				fromBankDid,
				default);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception occurred when sending ID Crypt invitation confirmation to bank '{FromBankDid}'", fromBankDid);
			throw;
		}

		if (sendResult is not SendResult.Success)
		{
			_logger.LogError("Error occurred when sending ID Crypt invitation confirmation to bank '{FromBankDid}'", fromBankDid);

			throw new RtgsSubscriberException($"Error occurred when sending ID Crypt invitation confirmation to bank '{fromBankDid}'");
		}

		_logger.LogDebug("Sent ID Crypt invitation confirmation to bank '{FromBankDid}'", fromBankDid);
	}

	private async Task InvokeUserHandler(string fromBankDid, ConnectionResponse connection)
	{
		var invitationNotification = new IdCryptBankInvitationNotificationV1
		{
			BankPartnerDid = fromBankDid,
			Alias = connection.Alias,
			ConnectionId = connection.ConnectionId
		};

		await _userHandler.HandleMessageAsync(invitationNotification);
	}
}
