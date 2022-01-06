using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.Subscriber.HandleMessageCommands;

/// <summary>
/// Interface to define a factory to create <see cref="IHandleMessageCommand"/> instances.
/// </summary>
public interface IHandleMessageCommandsFactory
{
	/// <summary>
	/// Creates commands for the supplied <see cref="IHandler"/> instances.
	/// </summary>
	/// <param name="handlers">The collection of <see cref="IHandler"/> instances to use.</param>
	/// <returns>The collection of created <see cref="IHandleMessageCommand"/> instances.</returns>
	IEnumerable<IHandleMessageCommand> CreateAll(IReadOnlyCollection<IHandler> handlers);
}
