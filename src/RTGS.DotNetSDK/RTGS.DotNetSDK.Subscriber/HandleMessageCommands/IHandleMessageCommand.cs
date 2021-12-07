using System.Threading.Tasks;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.HandleMessageCommands
{
	public interface IHandleMessageCommand
	{
		string InstructionType { get; }

		Task HandleAsync(RtgsMessage message);
	}
}