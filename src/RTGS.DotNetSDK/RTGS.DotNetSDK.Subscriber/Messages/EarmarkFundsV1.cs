using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Subscriber.Messages
{
	public class EarmarkFundsV1
	{
		public Guid LockId { get; init; }
		public CashAccount38 LiquidityPoolAccount { get; init; }
		public decimal Amount { get; init; }
	}
}
