using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;
using RTGS.DotNetSDK.Subscriber.Adapters;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Handlers.Internal;
using RTGS.Public.Messages.Subscriber;

namespace RTGS.DotNetSDK.Subscriber.HandleMessageCommands;

internal class HandleMessageCommandsFactory : IHandleMessageCommandsFactory
{
	private readonly IEnumerable<ICommandCreator> _commandCreators;

	public HandleMessageCommandsFactory(IEnumerable<IMessageAdapter> messageAdapters, IEnumerable<IInternalHandler> internalHandlers)
	{
		IEnumerable<IMessageAdapter> enumeratedMessageAdapters = messageAdapters.ToList();
		IEnumerable<IInternalHandler> enumeratedInternalHandlers = internalHandlers.ToList();

		_commandCreators = new List<ICommandCreator>
		{
			new CommandCreator<PayawayFundsV1, IPayawayFundsV1Handler, IMessageAdapter<PayawayFundsV1>>(enumeratedMessageAdapters),
			new CommandCreator<PayawayCompleteV1, IPayawayCompleteV1Handler, IMessageAdapter<PayawayCompleteV1>>(enumeratedMessageAdapters),
			new CommandCreator<MessageRejectV1, IMessageRejectV1Handler, IMessageAdapter<MessageRejectV1>>(enumeratedMessageAdapters),
			new CommandCreator<AtomicLockResponseV1, IAtomicLockResponseV1Handler, IMessageAdapter<AtomicLockResponseV1>>(enumeratedMessageAdapters),
			new CommandCreator<AtomicTransferResponseV1, IAtomicTransferResponseV1Handler, IMessageAdapter<AtomicTransferResponseV1>>(enumeratedMessageAdapters),
			new CommandCreator<AtomicTransferFundsV1, IAtomicTransferFundsV1Handler, IMessageAdapter<AtomicTransferFundsV1>>(enumeratedMessageAdapters),
			new CommandCreator<EarmarkFundsV1, IEarmarkFundsV1Handler, IMessageAdapter<EarmarkFundsV1>>(enumeratedMessageAdapters),
			new CommandCreator<EarmarkCompleteV1, IEarmarkCompleteV1Handler, IMessageAdapter<EarmarkCompleteV1>>(enumeratedMessageAdapters),
			new CommandCreator<EarmarkReleaseV1, IEarmarkReleaseV1Handler, IMessageAdapter<EarmarkReleaseV1>>(enumeratedMessageAdapters),
			new CommandCreator<BankPartnersResponseV1, IBankPartnersResponseV1Handler, IMessageAdapter<BankPartnersResponseV1>>(enumeratedMessageAdapters),
			new CommandCreator<IdCryptInvitationConfirmationV1, IIdCryptInvitationConfirmationV1Handler, IMessageAdapter<IdCryptInvitationConfirmationV1>>(enumeratedMessageAdapters),

			new InternalCommandCreator<
				IdCryptCreateInvitationRequestV1,
				IdCryptCreateInvitationNotificationV1,
				IIdCryptCreateInvitationRequestV1Handler,
				IIdCryptCreateInvitationNotificationV1Handler,
				IMessageAdapter<IdCryptCreateInvitationRequestV1>>(enumeratedMessageAdapters, enumeratedInternalHandlers),

			new InternalCommandCreator<
				IdCryptBankInvitationV1,
				IdCryptBankInvitationNotificationV1,
				IIdCryptBankInvitationV1Handler,
				IIdCryptBankInvitationNotificationV1Handler,
				IMessageAdapter<IdCryptBankInvitationV1>>(enumeratedMessageAdapters, enumeratedInternalHandlers)
		};
	}

	public IEnumerable<IHandleMessageCommand> CreateAll(IReadOnlyCollection<IHandler> userHandlers)
	{
		var commands = _commandCreators.Select(creator => creator.Create(userHandlers));

		return commands;
	}

	private class CommandCreator<TMessage, THandler, TMessageAdapter> : ICommandCreator
		where THandler : IHandler<TMessage>
		where TMessageAdapter : IMessageAdapter<TMessage>
	{
		private readonly TMessageAdapter _messageAdapter;

		public CommandCreator(IEnumerable<IMessageAdapter> messageAdapters)
		{
			_messageAdapter = messageAdapters.OfType<TMessageAdapter>().Single();
		}

		public IHandleMessageCommand Create(IReadOnlyCollection<IHandler> userHandlers)
		{
			var handler = userHandlers.OfType<THandler>().Single();

			return new HandleMessageCommand<TMessage>(_messageAdapter, handler);
		}
	}

	private class InternalCommandCreator<TReceivedMessage, THandledMessage, TInternalHandler, TUserHandler, TMessageAdapter> : ICommandCreator
		where TInternalHandler : IInternalHandler<TReceivedMessage, THandledMessage>
		where TUserHandler : IHandler<THandledMessage>
		where TMessageAdapter : IMessageAdapter<TReceivedMessage>
	{
		private readonly TMessageAdapter _messageAdapter;
		private readonly IEnumerable<IInternalHandler> _internalHandlers;

		public InternalCommandCreator(IEnumerable<IMessageAdapter> messageAdapters, IEnumerable<IInternalHandler> internalHandlers)
		{
			_messageAdapter = messageAdapters.OfType<TMessageAdapter>().Single();
			_internalHandlers = internalHandlers;
		}

		public IHandleMessageCommand Create(IReadOnlyCollection<IHandler> userHandlers)
		{
			var internalHandler = _internalHandlers.OfType<TInternalHandler>().Single();

			var userHandler = userHandlers.OfType<TUserHandler>().Single();

			internalHandler.SetUserHandler(userHandler);

			return new HandleMessageCommand<TReceivedMessage>(_messageAdapter, internalHandler);
		}
	}

	private interface ICommandCreator
	{
		IHandleMessageCommand Create(IReadOnlyCollection<IHandler> userHandlers);
	}
}
