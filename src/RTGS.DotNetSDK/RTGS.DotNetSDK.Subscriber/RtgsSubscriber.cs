using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using RTGS.DotNetSDK.Subscriber.Adapters;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber
{
	internal sealed class RtgsSubscriber : IRtgsSubscriber
	{
		private readonly Payment.PaymentClient _grpcClient;
		private readonly IEnumerable<IMessageAdapter> _messageAdapters;
		private Task _executingTask;
		private AsyncDuplexStreamingCall<RtgsMessageAcknowledgement, RtgsMessage> _fromRtgsCall;

		public RtgsSubscriber(Payment.PaymentClient grpcClient, IEnumerable<IMessageAdapter> messageAdapters)
		{
			_grpcClient = grpcClient;
			_messageAdapters = messageAdapters;
		}

		// TODO: what if called twice?
		public void Start(IEnumerable<IHandler> handlers) =>
			_executingTask = Execute(handlers);

		private async Task Execute(IEnumerable<IHandler> handlers)
		{
			_fromRtgsCall = _grpcClient.FromRtgsMessage();

			var handlersLookup = new Dictionary<string, Func<RtgsMessage, Task>>();

			var payawayFundsV1Handler = handlers.OfType<IPayawayFundsV1Handler>().First(); // work out how to get it...
			var payawayFundsMessageAdapter = _messageAdapters.OfType<IMessageAdapter<FIToFICustomerCreditTransferV10>>().First();
			handlersLookup.Add(payawayFundsMessageAdapter.InstructionType, message => payawayFundsMessageAdapter.HandleMessageAsync(message, payawayFundsV1Handler));

			var payawayCompleteV1Handler = handlers.OfType<IPayawayCompleteV1Handler>().First(); // work out how to get it...
			var payawayCompleteMessageAdapter = _messageAdapters.OfType<IMessageAdapter<BankToCustomerDebitCreditNotificationV09>>().First();
			handlersLookup.Add(payawayCompleteMessageAdapter.InstructionType, message => payawayCompleteMessageAdapter.HandleMessageAsync(message, payawayCompleteV1Handler));

			var messageRejectedV1Handler = handlers.OfType<IMessageRejectV1Handler>().First(); // work out how to get it...
			var messageRejectedMessageAdapter = _messageAdapters.OfType<IMessageAdapter<Admi00200101>>().First();
			handlersLookup.Add(messageRejectedMessageAdapter.InstructionType, message => messageRejectedMessageAdapter.HandleMessageAsync(message, messageRejectedV1Handler));

			await foreach (var message in _fromRtgsCall.ResponseStream.ReadAllAsync())
			{
				handlersLookup.TryGetValue(message.Header.InstructionType, out var handlerFunc);

				var acknowledgement = new RtgsMessageAcknowledgement
				{
					Header = new RtgsMessageHeader
					{
						CorrelationId = message.Header.CorrelationId,
						InstructionType = message.Header.InstructionType
					},
					Success = true
				};

				await _fromRtgsCall.RequestStream.WriteAsync(acknowledgement);

				await handlerFunc(message);
			}
		}

		// TODO: what if called without calling start?
		public async Task StopAsync()
		{
			// TODO: timeout?
			await CompleteAsyncEnumerables();
		}

		private async Task CompleteAsyncEnumerables()
		{
			if (_fromRtgsCall is not null)
			{
				await _fromRtgsCall.RequestStream.CompleteAsync();

				await _executingTask;

				_fromRtgsCall.Dispose();
				_fromRtgsCall = null;
			}
		}
	}
}
