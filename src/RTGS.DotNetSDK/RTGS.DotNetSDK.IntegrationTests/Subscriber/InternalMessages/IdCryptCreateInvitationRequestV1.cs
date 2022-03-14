namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalMessages;

public record IdCryptCreateInvitationRequestV1
{
	public string PartnerBankDid { get; init; }
}
