using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public abstract class BasePublisherActionData : IEnumerable<object[]>
	{
		public abstract IPublisherAction<AtomicLockRequest> AtomicLock { get; }
		public abstract IPublisherAction<AtomicTransferRequest> AtomicTransfer { get; }
		public abstract IPublisherAction<EarmarkConfirmation> EarmarkConfirmation { get; }
		public abstract IPublisherAction<AtomicTransferConfirmation> AtomicTransferConfirmation { get; }
		public abstract IPublisherAction<UpdateLedgerRequest> UpdateLedger { get; }
		public abstract IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate { get; }
		public abstract IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation { get; }

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
