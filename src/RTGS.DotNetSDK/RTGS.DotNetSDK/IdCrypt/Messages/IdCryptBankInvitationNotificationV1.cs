namespace RTGS.DotNetSDK.IdCrypt.Messages;

/// <summary>
/// Represents a notification of an ID Crypt invitation 
/// </summary>
public record IdCryptBankInvitationNotificationV1
{
	public string BankPartnerDid { get; init; }
	public string Alias { get; init; }
	public string ConnectionId { get; init; }
}
