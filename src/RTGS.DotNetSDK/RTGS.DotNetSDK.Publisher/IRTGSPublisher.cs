using RTGS.DotNetSDK.Publisher.Messages;
using System;
using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Publisher
{
	public interface IRtgsPublisher : IAsyncDisposable
	{
		Task<bool> SendAtomicLockRequestAsync(AtomicLockRequest message);
	}
}
