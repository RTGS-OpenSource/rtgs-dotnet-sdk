using System.Threading.Tasks;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber
{
	public interface IHandler
	{
		string InstructionType { get; } // TODO: rename to message identifier?

		// TODO: use cancellation tokens to ensure tests would eventually finish by timing out
		public Task HandleMessageAsync(RtgsMessage message);
	}
}
