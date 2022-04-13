﻿namespace RTGS.DotNetSDK.Publisher.IdCrypt.Messages;

/// <summary>
/// Represents a notification of an ID Crypt invitation 
/// </summary>
public record IdCryptBankInvitationNotificationV1
{
	/// <summary>
	/// The RTGS ID of the invited bank partner.
	/// </summary>
	public string BankPartnerDid { get; init; }

	/// <summary>
	/// The Alias of the invitation.
	/// </summary>
	public string Alias { get; init; }

	/// <summary>
	/// The ConnectionId of the invitation.
	/// </summary>
	public string ConnectionId { get; init; }
}