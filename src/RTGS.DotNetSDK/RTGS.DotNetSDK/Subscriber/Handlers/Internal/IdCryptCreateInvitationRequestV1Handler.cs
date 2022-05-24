using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.IdCrypt;
using RTGS.DotNetSDK.Publisher.IdCrypt;
using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;
using RTGS.DotNetSDK.Subscriber.Exceptions;
using RTGS.IDCrypt.Service.Contracts.Connection;

namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;

internal class IdCryptCreateInvitationRequestV1Handler : IIdCryptCreateInvitationRequestV1Handler
{
	private readonly ILogger<IdCryptCreateInvitationRequestV1Handler> _logger;
	private readonly IIdCryptServiceClient _idCryptServiceClient;
	private readonly IIdCryptPublisher _idCryptPublisher;

	public IdCryptCreateInvitationRequestV1Handler(
		ILogger<IdCryptCreateInvitationRequestV1Handler> logger,
		IIdCryptServiceClient idCryptServiceClient,
		IIdCryptPublisher idCryptPublisher)
	{
		_logger = logger;
		_idCryptServiceClient = idCryptServiceClient;
		_idCryptPublisher = idCryptPublisher;
	}

	public async Task HandleMessageAsync(IdCryptCreateInvitationRequestV1 createInvitationRequest)
	{
		var invitation = await CreateIdCryptInvitationAsync(createInvitationRequest.BankPartnerRtgsGlobalId);

		await SendInvitationToBankAsync(invitation, createInvitationRequest.BankPartnerRtgsGlobalId);
	}

	private async Task<CreateConnectionInvitationResponse> CreateIdCryptInvitationAsync(string toRtgsGlobalId)
	{
		try
		{
			var invitation = await _idCryptServiceClient.CreateConnectionInvitationForBankAsync(toRtgsGlobalId);

			return invitation;
		}
		catch (Exception innerException)
		{
			var exception = new RtgsSubscriberException(
				"Error occurred creating ID Crypt invitation",
				innerException);

			_logger.LogError(
				exception,
				"Error occurred when sending CreateConnectionInvitation request to ID Crypt Service for invitation from bank");

			throw exception;
		}
	}

	private async Task SendInvitationToBankAsync(
		CreateConnectionInvitationResponse invitation,
		string bankPartnerRtgsGlobalId)
	{
		var invitationAlias = invitation.Alias;

		_logger.LogDebug("Sending Invitation with alias {Alias} to Bank {BankPartnerRtgsGlobalId}", invitationAlias, bankPartnerRtgsGlobalId);

		var invitationMessage = new IdCryptInvitationV1
		{
			Alias = invitationAlias,
			Id = invitation.Invitation.Id,
			Label = invitation.Invitation.Label,
			RecipientKeys = invitation.Invitation.RecipientKeys,
			ServiceEndpoint = invitation.Invitation.ServiceEndpoint,
			Type = invitation.Invitation.Type,
			AgentPublicDid = invitation.AgentPublicDid
		};

		SendResult sendResult;
		try
		{
			sendResult = await _idCryptPublisher
				.SendIdCryptInvitationToBankAsync(invitationMessage, bankPartnerRtgsGlobalId, default);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception occurred when sending IdCrypt Invitation with alias {Alias} to Bank {BankPartnerRtgsGlobalId}", invitationAlias, bankPartnerRtgsGlobalId);
			throw;
		}

		if (sendResult is not SendResult.Success)
		{
			_logger.LogError("Error occurred when sending IdCrypt Invitation with alias {Alias} to Bank {BankPartnerRtgsGlobalId}", invitationAlias, bankPartnerRtgsGlobalId);

			throw new RtgsSubscriberException(
				$"Error occurred when sending IdCrypt Invitation with alias {invitationAlias} to Bank {bankPartnerRtgsGlobalId}");
		}

		_logger.LogDebug("Sent Invitation with alias {Alias} to Bank {BankPartnerRtgsGlobalId}", invitationAlias, bankPartnerRtgsGlobalId);
	}
}
