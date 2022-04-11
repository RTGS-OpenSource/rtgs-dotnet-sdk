namespace RTGS.DotNetSDK.Publisher.IdCrypt.Messages;

internal record IdCryptBankInvitationV1
{
	public string FromBankDid { get; init; }
	public IdCryptInvitationV1 Invitation { get; init; }
}
