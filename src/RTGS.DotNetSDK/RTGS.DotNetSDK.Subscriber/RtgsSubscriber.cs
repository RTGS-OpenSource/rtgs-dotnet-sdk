using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber
{
	internal sealed class RtgsSubscriber : IRtgsSubscriber
	{
		private readonly Payment.PaymentClient _grpcClient;
		private Task _executingTask;
		private AsyncDuplexStreamingCall<RtgsMessageAcknowledgement, RtgsMessage> _fromRtgsCall;

		public RtgsSubscriber(Payment.PaymentClient grpcClient)
		{
			_grpcClient = grpcClient;
		}

		// TODO: what if called twice?
		public void Start(IEnumerable<IHandler> handlers) =>
			_executingTask = Execute(handlers);

		private async Task Execute(IEnumerable<IHandler> handlers)
		{
			_fromRtgsCall = _grpcClient.FromRtgsMessage();

			var handlersLookup = handlers.ToDictionary(handler => handler.InstructionType, handler => handler);

			await foreach (var message in _fromRtgsCall.ResponseStream.ReadAllAsync())
			{
				handlersLookup.TryGetValue(message.Header.InstructionType, out var handler);

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

				// TODO: handler should only get strongly typed data?
				await handler.HandleMessageAsync(message);
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
