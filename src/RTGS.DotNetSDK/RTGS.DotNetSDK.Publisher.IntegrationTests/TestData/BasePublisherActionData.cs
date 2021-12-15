using System.Collections;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public abstract class BasePublisherActionData : IEnumerable<object[]>
	{
		public abstract IPublisherAction<AtomicLockRequestV1> AtomicLock { get; }
		public abstract IPublisherAction<AtomicTransferRequestV1> AtomicTransfer { get; }
		public abstract IPublisherAction<EarmarkConfirmationV1> EarmarkConfirmation { get; }
		public abstract IPublisherAction<AtomicTransferConfirmationV1> AtomicTransferConfirmation { get; }
		public abstract IPublisherAction<UpdateLedgerRequestV1> UpdateLedger { get; }
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
