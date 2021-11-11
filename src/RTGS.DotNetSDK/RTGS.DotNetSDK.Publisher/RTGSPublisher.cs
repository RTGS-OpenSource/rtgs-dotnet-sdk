using Grpc.Core;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.Public.Payment.V2;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Publisher
{
	public class RtgsPublisher : IRtgsPublisher
	{
		private readonly Payment.PaymentClient _paymentClient;
		private AsyncDuplexStreamingCall<RtgsMessage, RtgsMessageAcknowledgement> _toRtgsCall;
		private readonly ManualResetEventSlim _pendingAcknowledgementEvent = new();
		private Task _acknowledgementsTask;

		public RtgsPublisher(Payment.PaymentClient paymentClient)
		{
			_paymentClient = paymentClient;
		}

		public async Task<bool> SendAtomicLockRequestAsync(AtomicLockRequest message)
		{
			var grpcCallHeaders = new Metadata { new("bankdid", "test") };

			if (_toRtgsCall is null)
			{
				_toRtgsCall = _paymentClient.ToRtgsMessage(grpcCallHeaders);
				_acknowledgementsTask = StartWaitingForAcknowledgements();
			}

			_pendingAcknowledgementEvent.Reset();

			await _toRtgsCall.RequestStream.WriteAsync(new RtgsMessage
			{
				Data = JsonSerializer.Serialize(message),
				Header = new RtgsMessageHeader
				{
					InstructionType = "payment.lock.v1",
					CorrelationId = Guid.NewGuid().ToString()
				}
			});

			_pendingAcknowledgementEvent.Wait();

			return true;
		}

		private async Task StartWaitingForAcknowledgements()
		{
			await foreach (var _ in _toRtgsCall.ResponseStream.ReadAllAsync())
			{
				_pendingAcknowledgementEvent.Set();
			}
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

				await _acknowledgementsTask;

				_toRtgsCall.Dispose();
				_toRtgsCall = null;
			}
		}
	}
}
