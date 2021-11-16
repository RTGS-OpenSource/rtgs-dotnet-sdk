using Grpc.Core;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using RTGS.Public.Payment.V2;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Publisher
{
	internal sealed class RtgsPublisher : IRtgsPublisher
	{
		private readonly ILogger<RtgsPublisher> _logger;
		private readonly Payment.PaymentClient _paymentClient;
		private readonly ManualResetEventSlim _pendingAcknowledgementEvent = new(); // TODO: dispose
		private readonly RtgsClientOptions _options;
		private AsyncDuplexStreamingCall<RtgsMessage, RtgsMessageAcknowledgement> _toRtgsCall;
		private Task _waitForAcknowledgementsTask;
		private RtgsMessageAcknowledgement _acknowledgement;
		private string _correlationId;
		private bool _disposed;

		public RtgsPublisher(ILogger<RtgsPublisher> logger, Payment.PaymentClient paymentClient, RtgsClientOptions options)
		{
			_logger = logger;
			_paymentClient = paymentClient;
			_options = options;
		}

		public Task<SendResult> SendAtomicLockRequestAsync(AtomicLockRequest message, CancellationToken cancellationToken) =>
			SendRequestAsync(message, "payment.lock.v1", cancellationToken);

		public Task<SendResult> SendAtomicTransferRequestAsync(AtomicTransferRequest message, CancellationToken cancellationToken) =>
			SendRequestAsync(message, "payment.block.v1", cancellationToken);

		public Task<SendResult> SendEarmarkConfirmationAsync(EarmarkConfirmation message, CancellationToken cancellationToken) =>
			SendRequestAsync(message, "payment.earmarkconfirmation.v1", cancellationToken);

		public Task<SendResult> SendTransferConfirmationAsync(TransferConfirmation message, CancellationToken cancellationToken) =>
			SendRequestAsync(message, "payment.blockconfirmation.v1", cancellationToken);

		public Task<SendResult> SendUpdateLedgerRequestAsync(UpdateLedgerRequest message, CancellationToken cancellationToken) =>
			SendRequestAsync(message, "payment.update.ledger.v1", cancellationToken);

		public Task<SendResult> SendPayawayCreateAsync(FIToFICustomerCreditTransferV10 message, CancellationToken cancellationToken) =>
			SendRequestAsync(message, "payaway.create.v1", cancellationToken);

		public Task<SendResult> SendPayawayConfirmationAsync(BankToCustomerDebitCreditNotificationV09 message, CancellationToken cancellationToken) =>
			SendRequestAsync(message, "payaway.confirmation.v1", cancellationToken);

		private async Task<SendResult> SendRequestAsync<T>(T message, string instructionType, CancellationToken cancellationToken)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(RtgsPublisher));
			}

			// TODO: EXCLUSIVE LOCK START
			if (_toRtgsCall is null)
			{
				var grpcCallHeaders = new Metadata { new("bankdid", _options.BankDid) };

				try
				{
					_logger.LogInformation("Connecting to RTGS");

					_toRtgsCall = _paymentClient.ToRtgsMessage(grpcCallHeaders);

					_logger.LogInformation("Connected to RTGS");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to connect to RTGS");

					return SendResult.ConnectionError;
				}

				_waitForAcknowledgementsTask = WaitForAcknowledgements();
			}

			_pendingAcknowledgementEvent.Reset();

			_correlationId = Guid.NewGuid().ToString();

			try
			{
				_logger.LogInformation("Sending {messageType} to RTGS", typeof(T).Name);

				await _toRtgsCall.RequestStream.WriteAsync(new RtgsMessage
				{
					Data = JsonSerializer.Serialize(message),
					Header = new RtgsMessageHeader
					{
						InstructionType = instructionType,
						CorrelationId = _correlationId
					}
				});

				_logger.LogInformation("Sent {messageType} to RTGS", typeof(T).Name);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error sending {messageType} to RTGS", typeof(T).Name);

				return SendResult.ClientError;
			}

			var expectedAcknowledgementReceived = _pendingAcknowledgementEvent.Wait(_options.WaitForAcknowledgementDuration, cancellationToken);

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
			if (_toRtgsCall is not null)
			{
				await _toRtgsCall.RequestStream.CompleteAsync();

				await _waitForAcknowledgementsTask;

				_toRtgsCall.Dispose();
				_toRtgsCall = null;
			}

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
			GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

			_disposed = true;
		}
	}
}
