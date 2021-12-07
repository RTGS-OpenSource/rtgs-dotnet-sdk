using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RTGS.DotNetSDK.Subscriber.Messages;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData
{
	public abstract class BaseSubscriberActionData : IEnumerable<object[]>
	{
		public abstract ISubscriberAction<ISO20022.Messages.Pacs_008_001.V10.FIToFICustomerCreditTransferV10> PayawayFundsV1 { get; }
		public abstract ISubscriberAction<ISO20022.Messages.Camt_054_001.V09.BankToCustomerDebitCreditNotificationV09> PayawayCompleteV1 { get; }
		public abstract ISubscriberAction<ISO20022.Messages.Admi_002_001.V01.Admi00200101> MessageRejectedV1 { get; }
		public abstract ISubscriberAction<AtomicLockResponseV1> AtomicLockResponseV1 { get; }
		public abstract ISubscriberAction<AtomicTransferResponseV1> AtomicTransferResponseV1 { get; }
		public abstract ISubscriberAction<BlockFundsV1> BlockFundsV1 { get; }

		public IEnumerator<object[]> GetActions() =>
			GetType().GetProperties()
				.Select(propertyInfo => new[] { propertyInfo.GetValue(this) })
				.GetEnumerator();

		public IEnumerator<object[]> GetEnumerator() =>
			GetActions();

		IEnumerator IEnumerable.GetEnumerator() =>
			GetEnumerator();
	}
}
