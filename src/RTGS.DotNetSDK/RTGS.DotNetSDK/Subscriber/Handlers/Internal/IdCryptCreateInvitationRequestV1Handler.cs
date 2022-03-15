using IDCryptGlobal.Cloud.Agent.Identity;
using IDCryptGlobal.Cloud.Agent.Identity.Connection;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.IdCrypt;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.DotNetSDK.Subscriber.Messages;

namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;

internal class IdCryptCreateInvitationRequestV1Handler : IIdCryptCreateInvitationRequestV1Handler
{
	private readonly ILogger<IdCryptCreateInvitationRequestV1Handler> _logger;
	private readonly IIdentityClient _identityClient;
	private readonly IIdCryptPublisher _idCryptPublisher;

	public IdCryptCreateInvitationRequestV1Handler(
		ILogger<IdCryptCreateInvitationRequestV1Handler> logger,
		IIdentityClient identityClient,
		IIdCryptPublisher idCryptPublisher)
	{
		_logger = logger;
		_identityClient = identityClient;
		_idCryptPublisher = idCryptPublisher;
	}

	public IHandler<IdCryptCreateInvitationNotificationV1> UserHandler { get; set; }

	public async Task HandleMessageAsync(IdCryptCreateInvitationRequestV1 createInvitationRequest)
	{
		var alias = Guid.NewGuid().ToString();

		var invitation = await CreateIdCryptInvitationAsync(alias);

		await SendInvitationToRtgsAsync(alias, invitation, "", default);

		var invitationNotification = new IdCryptCreateInvitationNotificationV1
		{
			Alias = alias,
			ConnectionId = invitation.ConnectionID,
			PartnerBankDid = createInvitationRequest.PartnerBankDid
		};

		await UserHandler.HandleMessageAsync(invitationNotification);
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

		var sendResult = await _idCryptPublisher
			.SendIdCryptInvitationAsync(invitationMessage, cancellationToken);

		return sendResult;
	}

}
