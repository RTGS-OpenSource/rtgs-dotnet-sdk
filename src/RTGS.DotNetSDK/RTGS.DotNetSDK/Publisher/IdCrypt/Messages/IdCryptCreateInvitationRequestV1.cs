namespace RTGS.DotNetSDK.Publisher.IdCrypt.Messages;

internal record IdCryptCreateInvitationRequestV1
{
	public string BankPartnerRtgsGlobalId { get; init; }
}
