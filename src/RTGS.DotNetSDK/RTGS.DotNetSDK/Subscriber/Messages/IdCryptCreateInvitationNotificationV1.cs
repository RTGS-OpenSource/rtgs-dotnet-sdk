﻿namespace RTGS.DotNetSDK.Subscriber.Messages;

public record IdCryptCreateInvitationNotificationV1
{
	public string PartnerBankDid { get; init; }
	public string ConnectionId { get; init; }
	public string Alias { get; init; }
}
