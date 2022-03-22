namespace RTGS.DotNetSDK.IntegrationTests.InternalMessages;

public record IdCryptBankInvitationV1
{
	// TODO JLIQ - Should this be BankPartnerDid?
	public string FromBankDid { get; init; }
	public IdCryptInvitationV1 Invitation { get; init; }
}
