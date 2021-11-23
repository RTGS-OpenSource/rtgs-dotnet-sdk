using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Publisher
{
	internal sealed class RtgsPublisher : IRtgsPublisher
	{
		private readonly Payment.PaymentClient _paymentClient;
		private readonly ManualResetEventSlim _pendingAcknowledgementEvent = new();
		private readonly RtgsClientOptions _options;
		private readonly ILogger<RtgsPublisher> _logger;
		private readonly SemaphoreSlim _sendingSignal = new(1);
		private AsyncDuplexStreamingCall<RtgsMessage, RtgsMessageAcknowledgement> _toRtgsCall;
		private Task _waitForAcknowledgementsTask;
		private RtgsMessageAcknowledgement _acknowledgement;
		private string _correlationId;
		private bool _disposed;

		public RtgsPublisher(Payment.PaymentClient paymentClient, RtgsClientOptions options, ILogger<RtgsPublisher> logger)
		{
			_paymentClient = paymentClient;
			_options = options;
			_logger = logger;
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

		private async Task<SendResult> SendRequestAsync<T>(T message, string instructionType, CancellationToken cancellationToken, [CallerMemberName] string callingMethod = null)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(RtgsPublisher));
			}

			await _sendingSignal.WaitAsync(cancellationToken);

			try
			{
				if (_toRtgsCall is null)
				{
					var grpcCallHeaders = new Metadata { new("bankdid", _options.BankDid) };
					_toRtgsCall = _paymentClient.ToRtgsMessage(grpcCallHeaders);
					_waitForAcknowledgementsTask = WaitForAcknowledgements();
				}

				_pendingAcknowledgementEvent.Reset();

				_correlationId = Guid.NewGuid().ToString();

				_logger.LogInformation("Sending {MessageType} to RTGS ({CallingMethod})", typeof(T).Name, callingMethod);

				await _toRtgsCall.RequestStream.WriteAsync(new RtgsMessage
				{
					Data = JsonSerializer.Serialize(message),
					Header = new RtgsMessageHeader
					{
						InstructionType = instructionType,
						CorrelationId = _correlationId
					}
				});

				_logger.LogInformation("Sent {MessageType} to RTGS ({CallingMethod})", typeof(T).Name, callingMethod);

				var expectedAcknowledgementReceived = _pendingAcknowledgementEvent.Wait(_options.WaitForAcknowledgementDuration, cancellationToken);

				if (!expectedAcknowledgementReceived)
				{
					_logger.LogError("Timed out waiting for {MessageType} acknowledgement from RTGS ({CallingMethod})", typeof(T).Name, callingMethod);
					return SendResult.Timeout;
				}

				var logLevel = _acknowledgement!.Success ? LogLevel.Information : LogLevel.Error;
				_logger.Log(logLevel, "Received {MessageType} acknowledgement (success: {Success}) from RTGS ({CallingMethod})", typeof(T).Name, _acknowledgement!.Success, callingMethod);

				return _acknowledgement!.Success ? SendResult.Success : SendResult.ServerError;
			}
			finally
			{
				_sendingSignal.Release();
			}
		}

		private async Task WaitForAcknowledgements()
		{
			await foreach (var acknowledgement in _toRtgsCall.ResponseStream.ReadAllAsync())
			{
				if (acknowledgement.Header.CorrelationId == _correlationId)
				{
					_acknowledgement = acknowledgement;
					_pendingAcknowledgementEvent.Set();
				}
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

			_pendingAcknowledgementEvent?.Dispose();

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
			GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

			_disposed = true;
		}
	}
}
