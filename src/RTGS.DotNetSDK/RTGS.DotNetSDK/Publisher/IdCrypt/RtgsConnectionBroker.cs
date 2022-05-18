using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.IdCrypt;
using RTGS.DotNetSDK.Publisher.Exceptions;
using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;
using RTGS.IDCrypt.Service.Contracts.Connection;

namespace RTGS.DotNetSDK.Publisher.IdCrypt;

internal class RtgsConnectionBroker : IRtgsConnectionBroker
{
	private readonly ILogger<RtgsConnectionBroker> _logger;
	private readonly IIdCryptServiceClient _idCryptServiceClient;
	private readonly IIdCryptPublisher _idCryptPublisher;

	public RtgsConnectionBroker(
		ILogger<RtgsConnectionBroker> logger,
		IIdCryptServiceClient idCryptServiceClient,
		IIdCryptPublisher idCryptPublisher)
	{
		_logger = logger;
		_idCryptServiceClient = idCryptServiceClient;
		_idCryptPublisher = idCryptPublisher;
	}

	public async Task<SendInvitationResult> SendInvitationAsync(CancellationToken cancellationToken = default)
	{
		var invitation = await CreateIdCryptInvitationAsync();

		var sendToRtgsResult = await SendInvitationToRtgsAsync(invitation, cancellationToken);

		var sendInvitationResult = new SendInvitationResult
		{
			Alias = sendToRtgsResult is SendResult.Success ? invitation.Alias : null,
			ConnectionId = sendToRtgsResult is SendResult.Success ? invitation.ConnectionId : null,
			SendResult = sendToRtgsResult
		};

		return sendInvitationResult;
	}

	private async Task<CreateConnectionInvitationResponse> CreateIdCryptInvitationAsync()
	{
		try
		{
			var invitation = await _idCryptServiceClient.CreateConnectionAsync();

			return invitation;
		}
		catch (Exception innerException)
		{
			const string errorMessage = "Error occurred creating ID Crypt invitation";
			var exception = new RtgsPublisherException(errorMessage, innerException);

			_logger.LogError(exception, errorMessage);

			throw exception;
		}
	}

	private async Task<SendResult> SendInvitationToRtgsAsync(
		CreateConnectionInvitationResponse invitation,
		CancellationToken cancellationToken)
	{
		var invitationAlias = invitation.Alias;

		var invitationMessage = new IdCryptInvitationV1
		{
			Alias = invitation.Alias,
			Id = invitation.Invitation.Id,
			Label = invitation.Invitation.Label,
			RecipientKeys = invitation.Invitation.RecipientKeys,
			ServiceEndpoint = invitation.Invitation.ServiceEndpoint,
			Type = invitation.Invitation.Type,
			AgentPublicDid = invitation.AgentPublicDid
		};

		try
		{
			_logger.LogDebug("Sending Id Crypt Invitation with alias {Alias} to RTGS", invitationAlias);

			var sendResult = await _idCryptPublisher.SendIdCryptInvitationToRtgsAsync(invitationMessage, cancellationToken);

			_logger.LogDebug("Sending Id Crypt Invitation with alias {Alias} to RTGS", invitationAlias);

			return sendResult;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception occurred when sending Id Crypt Invitation with alias {Alias} to RTGS", invitationAlias);
			throw;
		}


	}
}
