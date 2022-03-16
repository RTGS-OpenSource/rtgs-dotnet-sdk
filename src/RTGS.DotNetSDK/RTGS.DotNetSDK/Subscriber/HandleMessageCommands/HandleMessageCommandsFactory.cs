﻿using RTGS.DotNetSDK.Subscriber.Adapters;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Handlers.Internal;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.HandleMessageCommands;

internal class HandleMessageCommandsFactory : IHandleMessageCommandsFactory
{
	private readonly IEnumerable<ICommandCreator> _commandCreators;
	private readonly IEnumerable<IInternalCommandCreator> _internalCommandCreators;
	private readonly IEnumerable<IInternalHandler> _internalHandlers;

	public HandleMessageCommandsFactory(IEnumerable<IMessageAdapter> messageAdapters, IEnumerable<IInternalHandler> internalHandlers)
	{
		IEnumerable<IMessageAdapter> enumeratedMessageAdapters = messageAdapters.ToList();

		_commandCreators = new List<ICommandCreator>
		{
			new CommandCreator<FIToFICustomerCreditTransferV10, IPayawayFundsV1Handler, IMessageAdapter<FIToFICustomerCreditTransferV10>>(enumeratedMessageAdapters),
			new CommandCreator<BankToCustomerDebitCreditNotificationV09, IPayawayCompleteV1Handler, IMessageAdapter<BankToCustomerDebitCreditNotificationV09>>(enumeratedMessageAdapters),
			new CommandCreator<Admi00200101, IMessageRejectV1Handler, IMessageAdapter<Admi00200101>>(enumeratedMessageAdapters),
			new CommandCreator<AtomicLockResponseV1, IAtomicLockResponseV1Handler, IMessageAdapter<AtomicLockResponseV1>>(enumeratedMessageAdapters),
			new CommandCreator<AtomicTransferResponseV1, IAtomicTransferResponseV1Handler, IMessageAdapter<AtomicTransferResponseV1>>(enumeratedMessageAdapters),
			new CommandCreator<AtomicTransferFundsV1, IAtomicTransferFundsV1Handler, IMessageAdapter<AtomicTransferFundsV1>>(enumeratedMessageAdapters),
			new CommandCreator<EarmarkFundsV1, IEarmarkFundsV1Handler, IMessageAdapter<EarmarkFundsV1>>(enumeratedMessageAdapters),
			new CommandCreator<EarmarkCompleteV1, IEarmarkCompleteV1Handler, IMessageAdapter<EarmarkCompleteV1>>(enumeratedMessageAdapters),
			new CommandCreator<EarmarkReleaseV1, IEarmarkReleaseV1Handler, IMessageAdapter<EarmarkReleaseV1>>(enumeratedMessageAdapters),
			new CommandCreator<BankPartnersResponseV1, IBankPartnersResponseV1Handler, IMessageAdapter<BankPartnersResponseV1>>(enumeratedMessageAdapters),
			new CommandCreator<IdCryptInvitationConfirmationV1, IIdCryptInvitationConfirmationV1Handler, IMessageAdapter<IdCryptInvitationConfirmationV1>>(enumeratedMessageAdapters),
		};

		_internalCommandCreators = new List<IInternalCommandCreator>
		{
			new InternalCommandCreator<IdCryptCreateInvitationRequestV1, IdCryptCreateInvitationNotificationV1, IIdCryptCreateInvitationRequestV1Handler, IIdCryptCreateInvitationNotificationV1Handler, IMessageAdapter<IdCryptCreateInvitationRequestV1>>(enumeratedMessageAdapters)
		};

		_internalHandlers = internalHandlers;
	}

	public IEnumerable<IHandleMessageCommand> CreateAll(IReadOnlyCollection<IHandler> userHandlers)
	{
		var userCommands = _commandCreators.Select(creator => creator.Create(userHandlers));
		var internalCommands = _internalCommandCreators.Select(creator => creator.Create(_internalHandlers.ToList(), userHandlers));

		var commands = userCommands.Concat(internalCommands);

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

	private interface ICommandCreator
	{
		IHandleMessageCommand Create(IReadOnlyCollection<IHandler> userHandlers);
	}

	private class InternalCommandCreator<TReceivedMessage, THandledMessage, TInternalHandler, TUserHandler, TMessageAdapter> : IInternalCommandCreator
		where TInternalHandler : IInternalHandler<TReceivedMessage, THandledMessage>
		where TUserHandler : IHandler<THandledMessage>
		where TMessageAdapter : IMessageAdapter<TReceivedMessage>
	{
		private readonly TMessageAdapter _messageAdapter;

		public InternalCommandCreator(IEnumerable<IMessageAdapter> messageAdapters)
		{
			_messageAdapter = messageAdapters.OfType<TMessageAdapter>().Single();
		}

		public IHandleMessageCommand Create(IReadOnlyCollection<IInternalHandler> internalHandlers, IReadOnlyCollection<IHandler> userHandlers)
		{
			var internalHandler = internalHandlers.OfType<TInternalHandler>().Single();

			var userHandler = userHandlers.OfType<TUserHandler>().Single();

			internalHandler.SetUserHandler(userHandler);

			return new HandleMessageCommand<TReceivedMessage>(_messageAdapter, internalHandler);
		}
	}

	private interface IInternalCommandCreator
	{
		IHandleMessageCommand Create(IReadOnlyCollection<IInternalHandler> internalHandlers, IReadOnlyCollection<IHandler> userHandlers);
	}
}
