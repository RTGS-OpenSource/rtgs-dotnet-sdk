using RTGS.ISO20022.Messages.Camt_054_001.V09;

namespace RTGS.DotNetSDK.Subscriber.Handlers
{
	public interface IPayawayCompleteV1Handler : IHandler<BankToCustomerDebitCreditNotificationV09> { }
}
