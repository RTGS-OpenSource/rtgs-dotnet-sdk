using RTGS.ISO20022.Messages.Camt_054_001.V09;

namespace RTGS.DotNetSDK.Subscriber.Handlers;

/// <summary>
/// Interface to define a <see cref="BankToCustomerDebitCreditNotificationV09"/> handler.
/// </summary>
public interface IPayawayCompleteV1Handler : IHandler<BankToCustomerDebitCreditNotificationV09> { }
