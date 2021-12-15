﻿using System;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	/// <summary>
	/// Used to represent the message sent to RTGS to indicate that the funds have been transferred (or not if <see cref="Success"/> = false)
	/// </summary>
	public class AtomicTransferConfirmationV1
	{
		/// <summary>
		/// The lock Id used to correlate this transfer with the original atomic lock request.
		/// </summary>
		public Guid LockId { get; init; }

		/// <summary>
		/// True to confirm, false otherwise.
		/// </summary>
		public bool Success { get; init; }
	}
}