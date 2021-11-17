using System;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	public record EarmarkConfirmation
	{
		public Guid LockId { get; init; }
		public bool Success { get; init; }
	}
}
