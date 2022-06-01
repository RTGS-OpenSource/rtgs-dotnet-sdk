using RTGS.DotNetSDK.Subscriber.InternalMessages;
using RTGS.Public.Messages.Subscriber;

namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;

internal interface IInitiatingBankEarmarkFundsV1Handler : IInternalForwardingHandler<InitiatingBankEarmarkFundsV1, EarmarkFundsV1>
{
}
