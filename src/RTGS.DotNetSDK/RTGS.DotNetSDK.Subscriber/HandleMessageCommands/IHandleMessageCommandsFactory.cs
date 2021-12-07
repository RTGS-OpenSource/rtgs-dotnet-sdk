using System.Collections.Generic;
using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.Subscriber.HandleMessageCommands
{
	public interface IHandleMessageCommandsFactory
	{
		IEnumerable<IHandleMessageCommand> CreateAll(IReadOnlyCollection<IHandler> handlers);
	}
}