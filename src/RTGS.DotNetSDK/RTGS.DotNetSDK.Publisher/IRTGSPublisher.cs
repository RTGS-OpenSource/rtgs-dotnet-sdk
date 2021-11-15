using System;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Publisher.Messages;

namespace RTGS.DotNetSDK.Publisher
{
	public interface IRtgsPublisher : IAsyncDisposable
	{
		Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequest message);
		Task<SendResult> SendAtomicTransferRequestAsync(AtomicTransferRequest message);
	}
}
