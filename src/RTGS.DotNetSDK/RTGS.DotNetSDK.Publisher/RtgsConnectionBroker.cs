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

	public async Task<SendInvitationResult> SendInvitationAsync(CancellationToken cancellationToken)
	{
		var alias = Guid.NewGuid().ToString();

		var idCryptResponse = await CreateIdCryptInvitationAsync(alias);
		var sendToRtgsResult = await SendInvitationToRtgsAsync(alias, idCryptResponse.Invitation, cancellationToken);

		var connectionId = sendToRtgsResult is SendResult.Success ?
			idCryptResponse.ConnectionID :
			null;

		var sendInvitationResult = new SendInvitationResult
		{
			ConnectionId = connectionId,
			SendResult = sendToRtgsResult
		};

		return sendInvitationResult;
	}

	private async Task<ConnectionInviteResponseModel> CreateIdCryptInvitationAsync(string alias)
	{
		var autoAccept = true;
		var multiUse = false;
		var usePublicDid = false;

		var idCryptIdentityConnectionClient = _identityClient.Connection;

		ConnectionInviteResponseModel response;

		_logger.LogDebug("Sending CreateInvitation request to ID Crypt Cloud Agent");

		try
		{
			response = await idCryptIdentityConnectionClient.CreateInvitation(
				alias,
				autoAccept,
				multiUse,
				usePublicDid);
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Error occurred when calling ID Crypt Cloud Agent");

			throw;
		}

		_logger.LogDebug("Sent CreateInvitation request to ID Crypt Cloud Agent");

		return response;
	}

	private async Task<SendResult> SendInvitationToRtgsAsync(
		string alias,
		ConnectionInvitation invitation,
		CancellationToken cancellationToken)
	{
		var invitationMessage = new IdCryptInvitationV1
		{
			Alias = alias,
			Id = invitation.ID,
			Label = invitation.Label,
			RecipientKeys = invitation.RecipientKeys,
			ServiceEndPoint = invitation.ServiceEndPoint,
			Type = invitation.Type
		};

		var sendResult = await _rtgsInternalPublisher
			.SendIdCryptInvitationAsync(invitationMessage, cancellationToken);

		return sendResult;
	}
}
