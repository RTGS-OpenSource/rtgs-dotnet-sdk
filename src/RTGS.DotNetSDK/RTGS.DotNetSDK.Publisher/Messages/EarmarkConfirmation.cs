using System;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	/// <summary>
	/// Used to represent the message sent to RTGS to indicate that the funds have been earmarked (or not if <see cref="Success"/> = <code>false</code>)
	/// </summary>
	public record EarmarkConfirmation
	{
		/// <summary>
		/// The lock Id
		/// </summary>
		public Guid LockId { get; init; }

		/// <summary>
		/// True when successful, false otherwise
		/// </summary>
		public bool Success { get; init; }
	}
}
