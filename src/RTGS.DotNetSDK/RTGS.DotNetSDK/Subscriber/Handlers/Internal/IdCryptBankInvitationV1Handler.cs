using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Publisher.IdCrypt;
using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;
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

		var connection = await AcceptInviteAsync(bankInvitation.Invitation, bankInvitation.FromRtgsGlobalId);

		_ = WaitForActiveConnectionAndSendConfirmation(connection.ConnectionId, bankInvitation.FromRtgsGlobalId);
	}

	private async Task<ConnectionResponse> AcceptInviteAsync(IdCryptInvitationV1 invitation, string fromRtgsGlobalId)
	{
		var connectionInvite = new ReceiveAndAcceptInvitationRequest
		{
			Alias = invitation.Alias,
			Label = invitation.Label,
			RecipientKeys = invitation.RecipientKeys.ToArray(),
			Id = invitation.Id,
			Type = invitation.Type,
			ServiceEndpoint = invitation.ServiceEndpoint
		};

		try
		{
			_logger.LogDebug("Sending ReceiveAcceptInvitation request to ID Crypt for invitation from bank {FromRtgsGlobalId}",
				fromRtgsGlobalId);

			var response = await _connectionsClient.ReceiveAndAcceptInvitationAsync(connectionInvite);

			_logger.LogDebug("Sent ReceiveAcceptInvitation request to ID Crypt for invitation from bank {FromRtgsGlobalId}",
				fromRtgsGlobalId);

			return response;
		}
		catch (Exception innerException)
		{
			var exception = new RtgsSubscriberException(
				$"Error occurred when sending ReceiveAcceptInvitation request to ID Crypt for invitation from bank {fromRtgsGlobalId}",
				innerException);

			_logger.LogError(
				exception,
				"Error occurred when sending ReceiveAcceptInvitation request to ID Crypt for invitation from bank {FromRtgsGlobalId}",
				fromRtgsGlobalId);

			throw exception;
		}
	}

	private async Task WaitForActiveConnectionAndSendConfirmation(string connectionId, string fromRtgsGlobalId)
	{
		_logger.LogDebug("Polling for connection state for invitation from bank {FromRtgsGlobalId}", fromRtgsGlobalId);

		ConnectionResponse connection;
		try
		{
			connection = await PollConnectionState(connectionId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex,
				"Error occurred when polling for connection state for invitation from bank {FromRtgsGlobalId}",
				fromRtgsGlobalId);
			throw;
		}

		_logger.LogDebug("Finished polling for connection state for invitation from bank {FromRtgsGlobalId}", fromRtgsGlobalId);

		await HandleInvitationConfirmation(fromRtgsGlobalId, connection);
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

			if (Math.Round(watch.Elapsed.TotalSeconds, 0) >= maxPollTime.TotalSeconds)
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
			_logger.LogDebug("Sending GetConnection request to ID Crypt");

			var connection = await _connectionsClient.GetConnectionAsync(connectionId);

			_logger.LogDebug("Sent GetConnection request to ID Crypt");

			return connection;
		}
		catch (Exception innerException)
		{
			const string errorMessage = "Error occurred when sending GetConnection request to ID Crypt";

			var exception = new RtgsSubscriberException(errorMessage, innerException);

			_logger.LogError(exception, errorMessage);

			throw exception;
		}
	}

	private async Task HandleInvitationConfirmation(string fromRtgsGlobalId, ConnectionResponse connection)
	{
		var agentPublicDid = await GetIdCryptAgentPublicDidAsync();

		await SendInvitationConfirmationAsync(connection.Alias, agentPublicDid, fromRtgsGlobalId);

		await InvokeUserHandler(fromRtgsGlobalId, connection);
	}

	private async Task<string> GetIdCryptAgentPublicDidAsync()
	{
		try
		{
			_logger.LogDebug("Sending GetPublicDid request to ID Crypt Cloud Agent");

			var publicDid = await _walletClient.GetPublicDidAsync();

			_logger.LogDebug("Sent GetPublicDid request to ID Crypt Cloud Agent");

			return publicDid;
		}
		catch (Exception innerException)
		{
			const string errorMessage = "Error occurred when sending GetPublicDid request to ID Crypt Cloud Agent";

			var exception = new RtgsSubscriberException(errorMessage, innerException);

			_logger.LogError(exception, errorMessage);

			throw exception;
		}
	}

	private async Task SendInvitationConfirmationAsync(string alias, string agentPublicDid, string fromRtgsGlobalId)
	{
		_logger.LogDebug("Sending ID Crypt invitation confirmation to bank {FromRtgsGlobalId}", fromRtgsGlobalId);

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
				fromRtgsGlobalId,
				default);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception occurred when sending ID Crypt invitation confirmation to bank {FromRtgsGlobalId}", fromRtgsGlobalId);
			throw;
		}

		if (sendResult is not SendResult.Success)
		{
			_logger.LogError("Error occurred when sending ID Crypt invitation confirmation to bank {FromRtgsGlobalId}", fromRtgsGlobalId);

			throw new RtgsSubscriberException($"Error occurred when sending ID Crypt invitation confirmation to bank {fromRtgsGlobalId}");
		}

		_logger.LogDebug("Sent ID Crypt invitation confirmation to bank {FromRtgsGlobalId}", fromRtgsGlobalId);
	}

	private async Task InvokeUserHandler(string fromRtgsGlobalId, ConnectionResponse connection)
	{
		var invitationNotification = new IdCryptBankInvitationNotificationV1
		{
			BankPartnerRtgsGlobalId = fromRtgsGlobalId,
			Alias = connection.Alias,
			ConnectionId = connection.ConnectionId
		};

		await _userHandler.HandleMessageAsync(invitationNotification);
	}
}
