using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData
{
	public abstract class BaseSubscriberActionData : IEnumerable<object[]>
	{
		public abstract ISubscriberAction<ISO20022.Messages.Pacs_008_001.V10.FIToFICustomerCreditTransferV10> PayawayFunds { get; }
		public abstract ISubscriberAction<ISO20022.Messages.Camt_054_001.V09.BankToCustomerDebitCreditNotificationV09> PayawayComplete { get; }
		public abstract ISubscriberAction<ISO20022.Messages.Admi_002_001.V01.Admi00200101> MessageRejected { get; }

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
