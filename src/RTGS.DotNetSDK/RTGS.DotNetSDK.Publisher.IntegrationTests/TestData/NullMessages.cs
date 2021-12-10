using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public static class NullMessages
	{
		public static readonly AtomicLockRequest AtomicLockRequest = null;

		public static readonly AtomicTransferRequest AtomicTransferRequest = null;

		public static readonly EarmarkConfirmation EarmarkConfirmation = null;

		public static readonly AtomicTransferConfirmation AtomicTransferConfirmation = null;

		public static readonly UpdateLedgerRequest UpdateLedgerRequest = null;

		public static readonly FIToFICustomerCreditTransferV10 PayawayCreate = null;

		public static readonly BankToCustomerDebitCreditNotificationV09 PayawayConfirmation = null;
	}
}
