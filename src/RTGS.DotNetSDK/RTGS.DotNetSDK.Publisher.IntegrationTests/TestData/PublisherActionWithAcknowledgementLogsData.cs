using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Publisher.IntegrationTests.Logging;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	//public class PublisherActionAcknowledgementLogsData : BasePublisherActionData
	//{
	//	public override IPublisherAction<AtomicLockRequest> AtomicLock => 
	//		new PublisherActionWithLogs<AtomicLockRequest>(
	//			PublisherActions.AtomicLock,
	//			new List<LogEntry> { 
	//				new LogEntry($"Sending {typeof(TRequest).Name} to RTGS ({nameof(publisherAction.Action)})"),
	//				new LogEntry("Sent {MessageType} to RTGS ({CallingMethod})"),
	//				new LogEntry(""),
	//			});

	//	public override IPublisherAction<AtomicTransferRequest> AtomicTransfer => new PublisherActionWithLogs<AtomicTransferRequest>(PublisherActions.AtomicTransfer, new List<LogEntry>());
	//	public override IPublisherAction<EarmarkConfirmation> EarmarkConfirmation => new PublisherActionWithLogs<EarmarkConfirmation>(PublisherActions.EarmarkConfirmation, new List<LogEntry>());
	//	public override IPublisherAction<TransferConfirmation> TransferConfirmation => new PublisherActionWithLogs<TransferConfirmation>(PublisherActions.TransferConfirmation, new List<LogEntry>());
	//	public override IPublisherAction<UpdateLedgerRequest> UpdateLedger => new PublisherActionWithLogs<UpdateLedgerRequest>(PublisherActions.UpdateLedger, new List<LogEntry>());
	//	public override IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate => new PublisherActionWithLogs<FIToFICustomerCreditTransferV10>(
	//		PublisherActions.PayawayCreate,
	//		new List<LogEntry>());

	//	public override IPublisherAction<BankToCustomerDebitCreditNotificationV09> PayawayConfirmation => new PublisherActionWithLogs<BankToCustomerDebitCreditNotificationV09>(
	//		PublisherActions.PayawayConfirmation,
	//		new List<LogEntry>());
	//}
}
