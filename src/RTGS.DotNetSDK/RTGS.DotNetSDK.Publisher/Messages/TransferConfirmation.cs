using System;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	/// <summary>
	/// The TransferConfirmation class
	/// </summary>
	public class TransferConfirmation
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
