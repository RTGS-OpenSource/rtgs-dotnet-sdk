using RTGS.DotNetSDK.Subscriber.Adapters;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.HandleMessageCommands;

internal class HandleMessageCommand<TMessage> : IHandleMessageCommand
{
	private readonly IMessageAdapter<TMessage> _messageAdapter;
	private readonly IHandler<TMessage> _handler;

	public HandleMessageCommand(IMessageAdapter<TMessage> messageAdapter, IHandler<TMessage> handler)
	{
		_messageAdapter = messageAdapter;
		_handler = handler;
	}

	public string MessageIdentifier => _messageAdapter.MessageIdentifier;

	public async Task HandleAsync(RtgsMessage rtgsMessage) =>
		await _messageAdapter.HandleMessageAsync(rtgsMessage, _handler);
}
