using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public class SubscriberActionSignedMessagesData : BaseSignedSubscriberActionData
{
	public override IVerifiableSubscriberAction<PayawayFundsV1, FIToFICustomerCreditTransferV10> PayawayFundsV1 =>
		SubscriberActions.PayawayFundsV1;

	public override IVerifiableSubscriberAction<MessageRejectV1, Dictionary<string, object>> MessageRejectV1 =>
		SubscriberActions.MessageRejectV1;

	public override IVerifiableSubscriberAction<PayawayCompleteV1, Dictionary<string, object>> PayawayCompleteV1 =>
		SubscriberActions.PayawayCompleteV1;

	public override IVerifiableSubscriberAction<AtomicLockApproveV2, Dictionary<string, object>> AtomicLockApproveV2IBAN =>
		SubscriberActions.AtomicLockApproveV2IBAN;

	public override IVerifiableSubscriberAction<AtomicLockApproveV2, Dictionary<string, object>> AtomicLockApproveV2OtherId =>
		SubscriberActions.AtomicLockApproveV2OtherId;
}
