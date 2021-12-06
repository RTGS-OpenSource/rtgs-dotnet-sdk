using System.Threading.Tasks;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.Adapters
{
	public interface IMessageAdapter
	{
		string InstructionType { get; } // TODO: rename to message identifier?
	}

	public interface IMessageAdapter<out TMessage> : IMessageAdapter
	{
		// TODO: use cancellation tokens to ensure tests would eventually finish by timing out
		Task HandleMessageAsync(RtgsMessage message, IHandler<TMessage> handler);
	}
}
