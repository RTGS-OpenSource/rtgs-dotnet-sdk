using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.IdCrypt;
using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;
using RTGS.DotNetSDK.Subscriber.Exceptions;
using RTGS.IDCrypt.Service.Contracts.Connection;

namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;

internal class IdCryptBankInvitationV1Handler : IIdCryptBankInvitationV1Handler
{
	private readonly ILogger<IdCryptBankInvitationV1Handler> _logger;
	private readonly IIdCryptServiceClient _idCryptServiceClient;

	public IdCryptBankInvitationV1Handler(
		ILogger<IdCryptBankInvitationV1Handler> logger,
		IIdCryptServiceClient idCryptServiceClient)
	{
		_logger = logger;
		_idCryptServiceClient = idCryptServiceClient;
	}

	public async Task HandleMessageAsync(IdCryptBankInvitationV1 bankInvitation) =>
		await AcceptInviteAsync(bankInvitation.Invitation, bankInvitation.FromRtgsGlobalId);

	private async Task AcceptInviteAsync(IdCryptInvitationV1 invitation, string fromRtgsGlobalId)
	{
		var request = new AcceptConnectionInvitationRequest
		{
			Id = invitation.Id,
			Type = invitation.Type,
			Alias = invitation.Alias,
			Label = invitation.Label,
			RecipientKeys = invitation.RecipientKeys.ToArray(),
			ServiceEndpoint = invitation.ServiceEndpoint
		};

		try
		{
			_logger.LogDebug("Sending AcceptConnectionAsync request to ID Crypt Service for invitation from bank {FromRtgsGlobalId}",
				fromRtgsGlobalId);

			await _idCryptServiceClient.AcceptConnectionAsync(request);

			_logger.LogDebug("Sent AcceptConnectionAsync request to ID Crypt Service for invitation from bank {FromRtgsGlobalId}",
				fromRtgsGlobalId);
		}
		catch (Exception innerException)
		{
			var exception = new RtgsSubscriberException(
				$"Error occurred when sending AcceptConnectionAsync request to ID Crypt Service for invitation from bank {fromRtgsGlobalId}",
				innerException);

			_logger.LogError(
				exception,
				"Error occurred when sending AcceptConnectionAsync request to ID Crypt Service for invitation from bank {FromRtgsGlobalId}",
				fromRtgsGlobalId);

			throw exception;
		}
	}
}
