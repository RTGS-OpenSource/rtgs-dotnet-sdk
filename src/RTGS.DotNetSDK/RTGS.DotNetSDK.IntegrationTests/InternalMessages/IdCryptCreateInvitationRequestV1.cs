namespace RTGS.DotNetSDK.IntegrationTests.InternalMessages;

public record IdCryptCreateInvitationRequestV1
{
	public string BankPartnerDid { get; init; }
}
