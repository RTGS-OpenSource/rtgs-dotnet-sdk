using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Publisher
{
	public class RtgsPublisher : IRtgsPublisher
	{
		private readonly Payment.PaymentClient _paymentClient;
		private readonly ManualResetEventSlim _pendingAcknowledgementEvent = new();
		private readonly RtgsClientOptions _options;
		private AsyncDuplexStreamingCall<RtgsMessage, RtgsMessageAcknowledgement> _toRtgsCall;
		private Task _waitForAcknowledgementsTask;
		private RtgsMessageAcknowledgement _acknowledgement;
		private string _correlationId;

		public RtgsPublisher(Payment.PaymentClient paymentClient, RtgsClientOptions options)
		{
			_paymentClient = paymentClient;
			_options = options;
		}

		public async Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequest message)
		{
			// TODO: EXCLUSIVE LOCK START
			if (_toRtgsCall is null)
			{
				var grpcCallHeaders = new Metadata { new("bankdid", _options.BankDid) };
				_toRtgsCall = _paymentClient.ToRtgsMessage(grpcCallHeaders);
				_waitForAcknowledgementsTask = WaitForAcknowledgements();
			}

			_pendingAcknowledgementEvent.Reset();

			_correlationId = Guid.NewGuid().ToString();

			await _toRtgsCall.RequestStream.WriteAsync(new RtgsMessage
			{
				Data = JsonSerializer.Serialize(message),
				Header = new RtgsMessageHeader
				{
					InstructionType = "payment.lock.v1",
					CorrelationId = _correlationId
				}
			});

			var expectedAcknowledgementReceived = _pendingAcknowledgementEvent.Wait(_options.WaitForAcknowledgementDuration);

			if (!expectedAcknowledgementReceived)
			{
				return SendResult.Timeout;
			}

			return _acknowledgement?.Success == true ? SendResult.Success : SendResult.ServerError;
			// TODO: EXCLUSIVE LOCK END
		}

		private async Task WaitForAcknowledgements()
		{
			await foreach (var acknowledgement in _toRtgsCall.ResponseStream.ReadAllAsync())
			{
				if (acknowledgement.Header.CorrelationId != _correlationId)
				{
					continue;
				}

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

				await _waitForAcknowledgementsTask;

				_toRtgsCall.Dispose();
				_toRtgsCall = null;
			}
		}
	}
}
