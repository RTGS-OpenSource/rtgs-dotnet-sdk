﻿namespace RTGS.DotNetSDK.IdCrypt.Messages;

internal record IdCryptCreateInvitationRequestV1
{
	public string BankPartnerDid { get; init; }
}
