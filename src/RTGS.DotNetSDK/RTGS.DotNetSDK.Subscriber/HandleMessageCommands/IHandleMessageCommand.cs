using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.HandleMessageCommands
{
	public interface IHandleMessageCommand
	{
		string MessageIdentifier { get; }

		Task HandleAsync(RtgsMessage rtgsMessage);
	}
}
