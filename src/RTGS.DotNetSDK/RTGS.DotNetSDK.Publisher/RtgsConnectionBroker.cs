using IDCryptGlobal.Cloud.Agent.Identity;
using IDCryptGlobal.Cloud.Agent.Identity.Connection;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Publisher.Messages;

namespace RTGS.DotNetSDK.Publisher;

internal class RtgsConnectionBroker : IRtgsConnectionBroker
{
	private readonly ILogger<RtgsConnectionBroker> _logger;
	private readonly IIdentityClient _identityClient;
	private readonly IRtgsInternalPublisher _rtgsInternalPublisher;

	public RtgsConnectionBroker(
		ILogger<RtgsConnectionBroker> logger,
		IIdentityClient identityClient,
		IRtgsInternalPublisher rtgsInternalPublisher)
	{
		_logger = logger;
		_identityClient = identityClient;
		_rtgsInternalPublisher = rtgsInternalPublisher;
	}

	public async Task<SendInvitationResult> SendInvitationAsync(CancellationToken cancellationToken = default)
	{
		var alias = Guid.NewGuid().ToString();

		var idCryptResponse = await CreateIdCryptInvitationAsync(alias);
		var agentPublicDid = await GetIdCryptAgentPublicDidAsync();
		var sendToRtgsResult = await SendInvitationToRtgsAsync(alias, idCryptResponse.Invitation, agentPublicDid, cancellationToken);

		var sendInvitationResult = new SendInvitationResult
		{
			Alias = sendToRtgsResult is SendResult.Success ? alias : null,
			ConnectionId = sendToRtgsResult is SendResult.Success ? idCryptResponse.ConnectionID : null,
			SendResult = sendToRtgsResult
		};

		return sendInvitationResult;
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

			_logger.LogDebug("Sent CreateInvitation request to ID Crypt Cloud Agent");

			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when sending CreateInvitation request to ID Crypt Cloud Agent");

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

		var sendResult = await _rtgsInternalPublisher
			.SendIdCryptInvitationAsync(invitationMessage, cancellationToken);

		return sendResult;
	}
}
