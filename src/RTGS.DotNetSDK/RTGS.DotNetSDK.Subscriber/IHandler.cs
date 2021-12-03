using System.Threading.Tasks;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber
{
	public interface IHandler
	{
		string InstructionType { get; } // TODO: rename to message identifier?

		Task HandleMessageAsync(RtgsMessage message);
	}
}
