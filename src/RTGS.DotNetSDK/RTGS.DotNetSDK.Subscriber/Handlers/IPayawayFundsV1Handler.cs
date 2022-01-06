using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.Handlers;

/// <summary>
/// Interface to define an <see cref="FIToFICustomerCreditTransferV10"/> handler.
/// </summary>
public interface IPayawayFundsV1Handler : IHandler<FIToFICustomerCreditTransferV10> { }
