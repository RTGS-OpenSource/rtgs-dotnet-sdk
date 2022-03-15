namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.InternalMessages;

public record IdCryptCreateInvitationRequestV1
{
	public string BankPartnerDid { get; init; }
}
