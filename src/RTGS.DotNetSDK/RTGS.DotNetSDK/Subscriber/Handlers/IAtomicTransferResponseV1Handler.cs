using RTGS.DotNetSDK.Subscriber.Messages;

namespace RTGS.DotNetSDK.Subscriber.Handlers;

/// <summary>
/// Interface to define an <see cref="AtomicTransferResponseV1"/> handler.
/// </summary>
public interface IAtomicTransferResponseV1Handler : IHandler<AtomicTransferResponseV1> { }
