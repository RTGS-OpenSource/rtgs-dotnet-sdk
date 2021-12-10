using System;
using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Subscriber.Messages
{
	public record EarmarkFundsV1
	{
		public Guid LockId { get; init; }
		public CashAccount38 LiquidityPoolAccount { get; init; }
		public decimal Amount { get; init; }
	}
}
