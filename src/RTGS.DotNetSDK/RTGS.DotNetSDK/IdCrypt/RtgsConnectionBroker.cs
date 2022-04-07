using IDCryptGlobal.Cloud.Agent.Identity;
using IDCryptGlobal.Cloud.Agent.Identity.Connection;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.IdCrypt.Messages;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.DotNetSDK.IdCrypt;

internal class RtgsConnectionBroker : IRtgsConnectionBroker
{
	private readonly ILogger<RtgsConnectionBroker> _logger;
	private readonly IIdentityClient _identityClient;
	private readonly IIdCryptPublisher _idCryptPublisher;
	private readonly IWalletClient _walletClient;

	public RtgsConnectionBroker(
		ILogger<RtgsConnectionBroker> logger,
		IIdentityClient identityClient,
		IIdCryptPublisher idCryptPublisher,
		IWalletClient walletClient)
	{
		_logger = logger;
		_identityClient = identityClient;
		_idCryptPublisher = idCryptPublisher;
		_walletClient = walletClient;
	}

	public async Task<SendInvitationResult> SendInvitationAsync(CancellationToken cancellationToken = default)
	{
		var alias = Guid.NewGuid().ToString();

		var idCryptResponse = await CreateIdCryptInvitationAsync(alias);
		var agentPublicDid = await GetIdCryptAgentPublicDidAsync(cancellationToken);
		var sendToRtgsResult = await SendInvitationToRtgsAsync(alias, idCryptResponse.Invitation, agentPublicDid, cancellationToken);

		var sendInvitationResult = new SendInvitationResult
		{
			Alias = sendToRtgsResult is SendResult.Success ? alias : null,
			ConnectionId = sendToRtgsResult is SendResult.Success ? idCryptResponse.ConnectionID : null,
			SendResult = sendToRtgsResult
		};

		return sendInvitationResult;
	}

	private async Task<string> GetIdCryptAgentPublicDidAsync(CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogDebug("Sending GetPublicDid request to ID Crypt Cloud Agent");

			var response = await _walletClient.GetPublicDidAsync(cancellationToken);

			_logger.LogDebug("Sent GetPublicDid request to ID Crypt Cloud Agent");

			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when sending GetPublicDid request to ID Crypt Cloud Agent");

			throw;
		}
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

	private async Task<SendResult> SendInvitationToRtgsAsync(
		string alias,
		ConnectionInvitation invitation,
		string agentPublicDid,
		CancellationToken cancellationToken)
	{
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

		var sendResult = await _idCryptPublisher
			.SendIdCryptInvitationToRtgsAsync(invitationMessage, cancellationToken);

		return sendResult;
	}
}
