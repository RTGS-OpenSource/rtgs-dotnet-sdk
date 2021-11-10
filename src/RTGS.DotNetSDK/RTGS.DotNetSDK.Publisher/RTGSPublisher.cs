using Grpc.Core;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.Public.Payment.V2;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Publisher
{
	public class RtgsPublisher : IRtgsPublisher
	{
		private readonly Payment.PaymentClient _paymentClient;
		private AsyncDuplexStreamingCall<RtgsMessage, RtgsMessageAcknowledgement> _toRtgsCall;

		public RtgsPublisher(Payment.PaymentClient paymentClient)
		{
			_paymentClient = paymentClient;
		}

		public async Task SendAtomicLockRequestAsync(AtomicLockRequest message)
		{
			var grpcCallHeaders = new Metadata { new("bankdid", "test") };

			_toRtgsCall = _paymentClient.ToRtgsMessage(grpcCallHeaders);

			var data = JsonSerializer.Serialize(message);

			await _toRtgsCall.RequestStream.WriteAsync(new RtgsMessage
			{
				Data = data,
				Header = new RtgsMessageHeader
				{
					InstructionType = "payment.lock.v1",
					CorrelationId = Guid.NewGuid().ToString()
				}
			});
		}

		public async ValueTask DisposeAsync()
		{
			await DisposeAsyncCore();
			GC.SuppressFinalize(this);
		}

		protected virtual async ValueTask DisposeAsyncCore()
		{
			if (_toRtgsCall is not null)
			{
				await _toRtgsCall.RequestStream.CompleteAsync();

				_toRtgsCall.Dispose();
				_toRtgsCall = null;
			}
		}
	}
}
