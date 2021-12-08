using System.Collections.Generic;
using System.Linq;
using RTGS.DotNetSDK.Subscriber.Adapters;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Messages;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.HandleMessageCommands
{
	internal class HandleMessageCommandsFactory : IHandleMessageCommandsFactory
	{
		private readonly IEnumerable<IImplementationFactory> _implementationFactories;

		public HandleMessageCommandsFactory(IEnumerable<IMessageAdapter> messageAdapters)
		{
			IEnumerable<IMessageAdapter> enumeratedMessageAdapters = messageAdapters.ToList();

			_implementationFactories = new List<IImplementationFactory>
			{
				new ImplementationFactory<FIToFICustomerCreditTransferV10, IPayawayFundsV1Handler, IMessageAdapter<FIToFICustomerCreditTransferV10>>(enumeratedMessageAdapters),
				new ImplementationFactory<BankToCustomerDebitCreditNotificationV09, IPayawayCompleteV1Handler, IMessageAdapter<BankToCustomerDebitCreditNotificationV09>>(enumeratedMessageAdapters),
				new ImplementationFactory<Admi00200101, IMessageRejectV1Handler, IMessageAdapter<Admi00200101>>(enumeratedMessageAdapters),
				new ImplementationFactory<AtomicLockResponseV1, IAtomicLockResponseV1Handler, IMessageAdapter<AtomicLockResponseV1>>(enumeratedMessageAdapters),
				new ImplementationFactory<AtomicTransferResponseV1, IAtomicTransferResponseV1Handler, IMessageAdapter<AtomicTransferResponseV1>>(enumeratedMessageAdapters),
				new ImplementationFactory<BlockFundsV1, IBlockFundsV1Handler, IMessageAdapter<BlockFundsV1>>(enumeratedMessageAdapters),
				new ImplementationFactory<EarmarkFundsV1, IEarmarkFundsV1Handler, IMessageAdapter<EarmarkFundsV1>>(enumeratedMessageAdapters),
				new ImplementationFactory<EarmarkCompleteV1, IEarmarkCompleteV1Handler, IMessageAdapter<EarmarkCompleteV1>>(enumeratedMessageAdapters),
				new ImplementationFactory<EarmarkReleaseV1, IEarmarkReleaseV1Handler, IMessageAdapter<EarmarkReleaseV1>>(enumeratedMessageAdapters)
			};
		}

		public IEnumerable<IHandleMessageCommand> CreateAll(IReadOnlyCollection<IHandler> handlers) =>
			_implementationFactories.Select(factory => factory.Create(handlers));


		// TODO: HandleMessageCommandCreator?
		private class ImplementationFactory<TMessage, THandler, TMessageAdapter> : IImplementationFactory
			where THandler : IHandler<TMessage>
			where TMessageAdapter : IMessageAdapter<TMessage>
		{
			private readonly TMessageAdapter _messageAdapter;

			public ImplementationFactory(IEnumerable<IMessageAdapter> messageAdapters)
			{
				_messageAdapter = messageAdapters.OfType<TMessageAdapter>().Single();
			}

			public IHandleMessageCommand Create(IReadOnlyCollection<IHandler> handlers)
			{
				var handler = handlers.OfType<THandler>().Single();

				return new HandleMessageCommand<TMessage>(_messageAdapter, handler);
			}
		}

		private interface IImplementationFactory
		{
			IHandleMessageCommand Create(IReadOnlyCollection<IHandler> handlers);
		}
	}
}
