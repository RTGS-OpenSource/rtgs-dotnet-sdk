using RTGS.DotNetSDK.Subscriber.InternalMessages;
using RTGS.Public.Messages.Subscriber;

namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;

internal interface IPartnerBankEarmarkFundsV1Handler : IInternalForwardingHandler<PartnerBankEarmarkFundsV1, EarmarkFundsV1>
{
}
