using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.Subscriber
{
	public interface IRtgsSubscriber : IAsyncDisposable
	{
		event EventHandler<ExceptionEventArgs> OnExceptionOccurred;

		void Start(IEnumerable<IHandler> handlers);

		Task StopAsync();
	}
}
