namespace RTGS.DotNetSDK.Publisher.IdCrypt.Messages;

internal record IdCryptCreateInvitationRequestV1
{
	public string BankPartnerDid { get; init; }

	public string BankPartnerRtgsGlobalId { get; init; }
}
