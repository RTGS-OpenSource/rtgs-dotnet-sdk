﻿using System.Threading.Tasks;
using RTGS.DotNetSDK.Subscriber.Adapters;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.HandleMessageCommands
{
	internal class HandleMessageCommand<TMessage> : IHandleMessageCommand
	{
		private readonly IMessageAdapter<TMessage> _messageAdapter;
		private readonly IHandler<TMessage> _handler;

		public HandleMessageCommand(IMessageAdapter<TMessage> messageAdapter, IHandler<TMessage> handler)
		{
			_messageAdapter = messageAdapter;
			_handler = handler;
		}

		public string InstructionType => _messageAdapter.InstructionType;

		public async Task HandleAsync(RtgsMessage message) =>
			await _messageAdapter.HandleMessageAsync(message, _handler);
	}
}