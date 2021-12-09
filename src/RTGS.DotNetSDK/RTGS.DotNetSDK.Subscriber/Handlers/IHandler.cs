using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Subscriber.Handlers
{
	public interface IHandler { }

	public interface IHandler<in TMessage> : IHandler
	{
		// TODO: use cancellation tokens to ensure tests would eventually finish by timing out
		Task HandleMessageAsync(TMessage message);
	}
}
