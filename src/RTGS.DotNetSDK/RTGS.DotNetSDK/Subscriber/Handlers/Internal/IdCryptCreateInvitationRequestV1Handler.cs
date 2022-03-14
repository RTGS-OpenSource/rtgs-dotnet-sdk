using IDCryptGlobal.Cloud.Agent.Identity;
using IDCryptGlobal.Cloud.Agent.Identity.Connection;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Subscriber.Messages;

namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;

internal class IdCryptCreateInvitationRequestV1Handler : IDependentHandler<IdCryptCreateInvitationRequestV1, IdCryptCreateInvitationNotificationV1>
{
	private readonly ILogger<IdCryptCreateInvitationRequestV1Handler> _logger;
	private readonly IIdentityClient _identityClient;

	public IdCryptCreateInvitationRequestV1Handler(
		ILogger<IdCryptCreateInvitationRequestV1Handler> logger,
		IIdentityClient identityClient)
	{
		_logger = logger;
		_identityClient = identityClient;
	}

	public IHandler<IdCryptCreateInvitationNotificationV1> DependentHandler { get; set; }

	public async Task HandleMessageAsync(IdCryptCreateInvitationRequestV1 createInvitationRequest)
	{
		var alias = Guid.NewGuid().ToString();

		var invitation = await CreateIdCryptInvitationAsync(alias);

		var invitationNotification = new IdCryptCreateInvitationNotificationV1
		{
			Alias = alias,
			ConnectionId = invitation.ConnectionID,
			PartnerBankDid = createInvitationRequest.PartnerBankDid
		};

		await DependentHandler.HandleMessageAsync(invitationNotification);
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
}
