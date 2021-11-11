using System;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Publisher.Messages;

namespace RTGS.DotNetSDK.Publisher
{
	public interface IRtgsPublisher : IAsyncDisposable
	{
		Task<bool> SendAtomicLockRequestAsync(AtomicLockRequest message);
	}
}
