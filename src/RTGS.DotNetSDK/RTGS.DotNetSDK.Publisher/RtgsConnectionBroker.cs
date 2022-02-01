using IDCryptGlobal.Cloud.Agent.Identity;
using RTGS.DotNetSDK.Publisher.Messages;

namespace RTGS.DotNetSDK.Publisher;

internal class RtgsConnectionBroker : IRtgsConnectionBroker
{
	private readonly IIdentityClient _identityClient;
	private readonly IRtgsInternalPublisher _rtgsInternalPublisher;

	public RtgsConnectionBroker(
		IIdentityClient identityClient,
		IRtgsInternalPublisher rtgsInternalPublisher)
	{
		_identityClient = identityClient;
		_rtgsInternalPublisher = rtgsInternalPublisher;
	}

	public async Task<string> SendInvitationAsync(CancellationToken cancellationToken)
	{
		var alias = Guid.NewGuid().ToString();
		var autoAccept = true;
		var multiUse = false;
		var usePublicDid = false;

		var connection = _identityClient.Connection;

		var response = await connection.CreateInvitation(
			alias,
			autoAccept,
			multiUse,
			usePublicDid);

		var invitation = response.Invitation;

		var invitationMessage = new IdCryptInvitationV1
		{
			Alias = alias,
			Id = invitation.ID,
			Label = invitation.Label,
			RecipientKeys = invitation.RecipientKeys,
			ServiceEndPoint = invitation.ServiceEndPoint,
			Type = invitation.Type
		};

		await _rtgsInternalPublisher
			.SendIdCryptInvitationAsync(invitationMessage, cancellationToken);

		return response.ConnectionID;
	}
}
