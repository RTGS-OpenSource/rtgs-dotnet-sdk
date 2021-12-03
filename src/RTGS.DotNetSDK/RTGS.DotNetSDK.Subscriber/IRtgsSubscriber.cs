using System.Collections.Generic;
using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Subscriber
{
	public interface IRtgsSubscriber
	{
		void Start(IEnumerable<IHandler> handlers);
		Task StopAsync();
	}
}
