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
		private readonly CancellationTokenSource _sharedTokenSource = new();
		private readonly RtgsClientOptions _options;
		private readonly ILogger<RtgsPublisher> _logger;
		private readonly SemaphoreSlim _sendingSignal = new(1);
		private readonly SemaphoreSlim _disposingSignal = new(1);
		private AcknowledgementContext _acknowledgementContext;
		private AsyncDuplexStreamingCall<RtgsMessage, RtgsMessageAcknowledgement> _toRtgsCall;
		private Task _waitForAcknowledgementsTask;
		private bool _disposed;
		private bool _recycleConnection;

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

			using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_sharedTokenSource.Token, cancellationToken);
			await _sendingSignal.WaitAsync(linkedTokenSource.Token);

			try
			{
				linkedTokenSource.Token.ThrowIfCancellationRequested();

				await EnsureRtgsCallSetup<T>();

				_acknowledgementContext = new AcknowledgementContext();

				await SendMessage(message, instructionType, callingMethod);

				await _acknowledgementContext.WaitAsync(_options.WaitForAcknowledgementDuration, linkedTokenSource.Token);

				LogAcknowledgementResult<T>(callingMethod);

				return _acknowledgementContext.Status;
			}
			finally
			{
				if (_acknowledgementContext != null)
				{
					_acknowledgementContext.Dispose();
					_acknowledgementContext = null;
				}
				_sendingSignal.Release();
			}
		}

		private async Task EnsureRtgsCallSetup<T>()
		{
			if (_recycleConnection)
			{
				_toRtgsCall?.Dispose();
				_toRtgsCall = null;

				_recycleConnection = false;
			}

			if (_toRtgsCall is null)
			{
				var grpcCallHeaders = new Metadata { new("bankdid", _options.BankDid) };
				_toRtgsCall = _paymentClient.ToRtgsMessage(grpcCallHeaders);

				if (_waitForAcknowledgementsTask is not null)
				{
					await _waitForAcknowledgementsTask;
				}

				_waitForAcknowledgementsTask = WaitForAcknowledgements();
			}
		}

		private async Task WaitForAcknowledgements()
		{
			try
			{
				await foreach (var acknowledgement in _toRtgsCall.ResponseStream.ReadAllAsync())
				{
					if (acknowledgement.Header.CorrelationId == _acknowledgementContext?.CorrelationId)
					{
						_acknowledgementContext?.Release(acknowledgement);
					}
				}
			}
			catch (RpcException ex)
			{
				if (_acknowledgementContext is null)
				{
					// TODO: log?
				}

				await _toRtgsCall.RequestStream.CompleteAsync();

				_recycleConnection = true;

				_acknowledgementContext?.Release(ex);
			}
		}

		private async Task SendMessage<T>(T message, string instructionType, string callingMethod)
		{
			_logger.LogInformation("Sending {MessageType} to RTGS ({CallingMethod})", typeof(T).Name, callingMethod);

			await _toRtgsCall.RequestStream.WriteAsync(new RtgsMessage
			{
				Data = JsonSerializer.Serialize(message),
				Header = new RtgsMessageHeader
				{
					InstructionType = instructionType,
					CorrelationId = _acknowledgementContext.CorrelationId
				}
			});

			_logger.LogInformation("Sent {MessageType} to RTGS ({CallingMethod})", typeof(T).Name, callingMethod);
		}

		private void LogAcknowledgementResult<T>(string callingMethod)
		{
			switch (_acknowledgementContext.Status)
			{
				case SendResult.Success:
					_logger.LogInformation("Received {MessageType} acknowledgement (acknowledged) from RTGS ({CallingMethod})", typeof(T).Name, callingMethod);
					break;

				case SendResult.Timeout:
					_logger.LogError("Timed out waiting for {MessageType} acknowledgement from RTGS ({CallingMethod})", typeof(T).Name, callingMethod);
					break;

				case SendResult.Rejected:
					_logger.LogError("Received {MessageType} acknowledgement (rejected) from RTGS ({CallingMethod})", typeof(T).Name, callingMethod);
					break;

				case SendResult.ServerError:
					_logger.LogError(_acknowledgementContext.RpcException, "Error received when sending {MessageType} to RTGS ({CallingMethod})", typeof(T).Name, callingMethod);
					break;

				case SendResult.Unknown:
				default:
					// TODO: log??
					break;
			}
		}

		public async ValueTask DisposeAsync()
		{
			if (_disposed)
			{
				return;
			}

			await _disposingSignal.WaitAsync();

			try
			{
				if (_disposed)
				{
					return;
				}

				_disposed = true;

				_sharedTokenSource.Cancel();

				if (_toRtgsCall is not null)
				{
					await _toRtgsCall.RequestStream.CompleteAsync();

					await _waitForAcknowledgementsTask;

					_toRtgsCall.Dispose();
					_toRtgsCall = null;
				}

				_acknowledgementContext?.Dispose();
				_sharedTokenSource.Dispose();
			}
			finally
			{
				_disposingSignal.Release();
			}

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
			GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
		}

		private sealed class AcknowledgementContext : IDisposable
		{
			private SemaphoreSlim _acknowledgementSignal;

			public AcknowledgementContext()
			{
				_acknowledgementSignal = new SemaphoreSlim(0, 1);
				CorrelationId = Guid.NewGuid().ToString();
			}

			public string CorrelationId { get; }
			public RpcException RpcException { get; private set; }
			public SendResult Status { get; private set; }

			public async Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
			{
				var enteredSemaphore = await _acknowledgementSignal.WaitAsync(timeout, cancellationToken);
				if (!enteredSemaphore)
				{
					Status = SendResult.Timeout;
				}
			}

			public void Release(RtgsMessageAcknowledgement acknowledgement)
			{
				_acknowledgementSignal?.Release();
				Status = acknowledgement.Success ? SendResult.Success : SendResult.Rejected;
			}

			public void Release(RpcException exception)
			{
				RpcException = exception;
				_acknowledgementSignal?.Release();
				Status = SendResult.ServerError;
			}

			public void Dispose()
			{
				_acknowledgementSignal.Dispose();
				_acknowledgementSignal = null;
			}
		}
	}
}
