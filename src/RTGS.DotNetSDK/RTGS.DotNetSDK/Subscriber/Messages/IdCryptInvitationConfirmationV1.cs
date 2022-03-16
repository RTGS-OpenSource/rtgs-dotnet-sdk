﻿namespace RTGS.DotNetSDK.Subscriber.Messages;

/// <summary>
/// Represents a confirmation for an accepted invitation.
/// </summary>
public record IdCryptInvitationConfirmationV1
{
	/// <summary>
	/// The Alias of the accepted and confirmed invitation.
	/// </summary>
	public string Alias { get; init; }
}

