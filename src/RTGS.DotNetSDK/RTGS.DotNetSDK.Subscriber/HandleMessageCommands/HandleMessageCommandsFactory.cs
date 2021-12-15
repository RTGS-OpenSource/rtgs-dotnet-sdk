using RTGS.DotNetSDK.Subscriber.Adapters;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.HandleMessageCommands;

internal class HandleMessageCommandsFactory : IHandleMessageCommandsFactory
{
	private readonly IEnumerable<ICommandCreator> _commandCreators;

	public HandleMessageCommandsFactory(IEnumerable<IMessageAdapter> messageAdapters)
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
			new CommandCreator<EarmarkReleaseV1, IEarmarkReleaseV1Handler, IMessageAdapter<EarmarkReleaseV1>>(enumeratedMessageAdapters)
		};
	}

	public IEnumerable<IHandleMessageCommand> CreateAll(IReadOnlyCollection<IHandler> handlers) =>
		_commandCreators.Select(creator => creator.Create(handlers));

	private class CommandCreator<TMessage, THandler, TMessageAdapter> : ICommandCreator
		where THandler : IHandler<TMessage>
		where TMessageAdapter : IMessageAdapter<TMessage>
	{
		private readonly TMessageAdapter _messageAdapter;

		public CommandCreator(IEnumerable<IMessageAdapter> messageAdapters)
		{
			_messageAdapter = messageAdapters.OfType<TMessageAdapter>().Single();
		}

		public IHandleMessageCommand Create(IReadOnlyCollection<IHandler> handlers)
		{
			var handler = handlers.OfType<THandler>().Single();

			return new HandleMessageCommand<TMessage>(_messageAdapter, handler);
		}
	}

	private interface ICommandCreator
	{
		IHandleMessageCommand Create(IReadOnlyCollection<IHandler> handlers);
	}
}
