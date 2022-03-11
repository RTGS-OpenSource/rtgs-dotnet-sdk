using RTGS.DotNetSDK.Subscriber.Messages;

namespace RTGS.DotNetSDK.Subscriber.Handlers;

/// <summary>
/// Interface to define an <see cref="AtomicLockResponseV1"/> handler.
/// </summary>
public interface IAtomicLockResponseV1Handler : IHandler<AtomicLockResponseV1> { }
