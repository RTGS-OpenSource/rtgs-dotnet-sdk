using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;
using RTGS.DotNetSDK.Subscriber.Adapters;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Handlers.Internal;
using RTGS.DotNetSDK.Subscriber.InternalMessages;
using RTGS.Public.Messages.Subscriber;

namespace RTGS.DotNetSDK.Subscriber.HandleMessageCommands;

internal class HandleMessageCommandsFactory : IHandleMessageCommandsFactory
{
	private readonly IEnumerable<ICommandCreator> _commandCreators;
	private readonly IReadOnlyCollection<IInternalHandler> _internalHandlers;

	public HandleMessageCommandsFactory(IEnumerable<IMessageAdapter> messageAdapters, IEnumerable<IInternalHandler> internalHandlers)
	{
		_internalHandlers = internalHandlers.ToList();
		IEnumerable<IMessageAdapter> enumeratedMessageAdapters = messageAdapters.ToList();

		_commandCreators = new List<ICommandCreator>
		{
			new CommandCreator<PayawayFundsV1, IPayawayFundsV1Handler, IMessageAdapter<PayawayFundsV1>>(enumeratedMessageAdapters),
			new CommandCreator<PayawayCompleteV1, IPayawayCompleteV1Handler, IMessageAdapter<PayawayCompleteV1>>(enumeratedMessageAdapters),
			new CommandCreator<MessageRejectV1, IMessageRejectV1Handler, IMessageAdapter<MessageRejectV1>>(enumeratedMessageAdapters),
			new CommandCreator<AtomicLockResponseV1, IAtomicLockResponseV1Handler, IMessageAdapter<AtomicLockResponseV1>>(enumeratedMessageAdapters),
			new CommandCreator<AtomicTransferResponseV1, IAtomicTransferResponseV1Handler, IMessageAdapter<AtomicTransferResponseV1>>(enumeratedMessageAdapters),
			new CommandCreator<AtomicTransferFundsV1, IAtomicTransferFundsV1Handler, IMessageAdapter<AtomicTransferFundsV1>>(enumeratedMessageAdapters),
			// TODO JLIQ - Should be removed once internal changes have been made
			new CommandCreator<EarmarkFundsV1, IEarmarkFundsV1Handler, IMessageAdapter<EarmarkFundsV1>>(enumeratedMessageAdapters),
			new CommandCreator<EarmarkCompleteV1, IEarmarkCompleteV1Handler, IMessageAdapter<EarmarkCompleteV1>>(enumeratedMessageAdapters),
			new CommandCreator<EarmarkReleaseV1, IEarmarkReleaseV1Handler, IMessageAdapter<EarmarkReleaseV1>>(enumeratedMessageAdapters),
			new CommandCreator<BankPartnersResponseV1, IBankPartnersResponseV1Handler, IMessageAdapter<BankPartnersResponseV1>>(enumeratedMessageAdapters),

			new InternalCommandCreator<
				InitiatingBankEarmarkFundsV1,
				EarmarkFundsV1,
				IInitiatingBankEarmarkFundsV1Handler,
				IEarmarkFundsV1Handler,
				IMessageAdapter<InitiatingBankEarmarkFundsV1>>(enumeratedMessageAdapters, _internalHandlers),

			new InternalCommandCreator<
				PartnerBankEarmarkFundsV1,
				EarmarkFundsV1,
				IPartnerBankEarmarkFundsV1Handler,
				IEarmarkFundsV1Handler,
				IMessageAdapter<PartnerBankEarmarkFundsV1>>(enumeratedMessageAdapters, _internalHandlers),

			new CommandCreator<IdCryptCreateInvitationRequestV1, IIdCryptCreateInvitationRequestV1Handler, IMessageAdapter<IdCryptCreateInvitationRequestV1>>(enumeratedMessageAdapters),
			new CommandCreator<IdCryptBankInvitationV1, IIdCryptBankInvitationV1Handler, IMessageAdapter<IdCryptBankInvitationV1>>(enumeratedMessageAdapters),
			new CommandCreator<AtomicLockApproveV2, IAtomicLockApproveV2Handler, IMessageAdapter<AtomicLockApproveV2>>(enumeratedMessageAdapters)
		};
	}

	public IEnumerable<IHandleMessageCommand> CreateAll(IReadOnlyCollection<IHandler> handlers) =>
		_commandCreators.Select(creator => creator.Create(handlers.Concat(_internalHandlers).ToList()));

	private sealed class CommandCreator<TMessage, THandler, TMessageAdapter> : ICommandCreator
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

	private sealed class InternalCommandCreator<TReceivedMessage, THandledMessage, TInternalHandler, TUserHandler, TMessageAdapter> : ICommandCreator
		where TInternalHandler : IInternalForwardingHandler<TReceivedMessage, THandledMessage>
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
