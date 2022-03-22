namespace RTGS.DotNetSDK.IntegrationTests.InternalMessages;

public record IdCryptBankInvitationV1
{
	public string FromBankDid { get; init; }
	public IdCryptInvitationV1 Invitation { get; init; }
}
