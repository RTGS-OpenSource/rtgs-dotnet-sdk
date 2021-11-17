using System;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	public class TransferConfirmation
	{
		public Guid LockId { get; init; }
		public bool Success { get; init; }
	}
}
