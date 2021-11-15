using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	public record EarmarkConfirmation
	{
		public Guid LockId { get; init; }
		public bool Success { get; init; }
	}
}
