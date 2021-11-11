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
		private readonly ManualResetEventSlim _pendingAcknowledgementEvent = new();
		private readonly RtgsClientOptions _options;
		private AsyncDuplexStreamingCall<RtgsMessage, RtgsMessageAcknowledgement> _toRtgsCall;
		private Task _acknowledgementsTask;
		private RtgsMessageAcknowledgement _acknowledgement;

		public RtgsPublisher(Payment.PaymentClient paymentClient, RtgsClientOptions options)
		{
			_paymentClient = paymentClient;
			_options = options;
		}

		public async Task<bool> SendAtomicLockRequestAsync(AtomicLockRequest message)
		{
			if (_toRtgsCall is null)
			{
				var grpcCallHeaders = new Metadata { new("bankdid", _options.BankDid) };
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

			var acknowledgementSet = _pendingAcknowledgementEvent.Wait(_options.WaitForAcknowledgementDuration);

			return acknowledgementSet && _acknowledgement?.Success == true;
		}

		private async Task StartWaitingForAcknowledgements()
		{
			await foreach (var acknowledgement in _toRtgsCall.ResponseStream.ReadAllAsync())
			{
				_acknowledgement = acknowledgement;
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
