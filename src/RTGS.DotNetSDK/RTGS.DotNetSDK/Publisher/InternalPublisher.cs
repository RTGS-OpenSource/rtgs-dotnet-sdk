﻿using System.Runtime.CompilerServices;
using System.Text.Json;
using Google.Protobuf.Collections;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Publisher.IdCrypt.Signing;
using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Publisher;

internal class InternalPublisher : IInternalPublisher
{
	private readonly ILogger<InternalPublisher> _logger;
	private readonly Payment.PaymentClient _paymentClient;
	private readonly RtgsSdkOptions _options;
	private readonly IServiceProvider _serviceProvider;
	private readonly CancellationTokenSource _sharedTokenSource = new();
	private readonly SemaphoreSlim _sendingSignal = new(1);
	private readonly SemaphoreSlim _disposingSignal = new(1);
	private readonly SemaphoreSlim _writingSignal = new(1);
	private AcknowledgementContext _acknowledgementContext;
	private AsyncDuplexStreamingCall<RtgsMessage, RtgsMessageAcknowledgement> _toRtgsCall;
	private Task _waitForAcknowledgementsTask;
	private bool _disposed;
	private bool _resetConnection;

	public InternalPublisher(ILogger<InternalPublisher> logger, Payment.PaymentClient paymentClient, RtgsSdkOptions options, IServiceProvider serviceProvider)
	{
		_logger = logger;
		_paymentClient = paymentClient;
		_options = options;
		_serviceProvider = serviceProvider;
	}

	public async Task<SendResult> SendMessageAsync<T>(
		T message,
		string messageIdentifier,
		CancellationToken cancellationToken,
		Dictionary<string, string> headers = null,
		[CallerMemberName] string callingMethod = null,
		string idCryptAlias = null)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(RtgsPublisher));
		}

		ArgumentNullException.ThrowIfNull(message, nameof(message));

		using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_sharedTokenSource.Token, cancellationToken);
		await _sendingSignal.WaitAsync(linkedTokenSource.Token);

		try
		{
			linkedTokenSource.Token.ThrowIfCancellationRequested();

			await EnsureRtgsCallSetup(linkedTokenSource.Token);

			_acknowledgementContext = new AcknowledgementContext();

			await SendMessageAsync(message, messageIdentifier, headers, callingMethod, idCryptAlias, linkedTokenSource.Token);

			await _acknowledgementContext.WaitAsync(_options.WaitForAcknowledgementDuration, linkedTokenSource.Token);

			LogAcknowledgementResult<T>(callingMethod);

			if (_acknowledgementContext.RpcException is not null)
			{
				throw _acknowledgementContext.RpcException;
			}

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

	private async Task SignMessageAsync<TMessageType>(TMessageType message, string idCryptAlias, MapField<string, string> headers)
	{
		var messageType = message.GetType();

		var messageSignerType = typeof(ISignMessage<>)
			.MakeGenericType(messageType);

		var messageSigner = _serviceProvider
			.GetService(messageSignerType) as ISignMessage<TMessageType>;

		if (messageSigner is null)
		{
			return;
		} 

		var signatures = await messageSigner.SignAsync(message, idCryptAlias);

		headers.Add("pairwise-did-signature", signatures.PairwiseDidSignature);
		headers.Add("public-did-signature", signatures.PublicDidSignature);
	}

	private async Task EnsureRtgsCallSetup(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (_resetConnection)
		{
			if (_toRtgsCall is not null)
			{
				await _writingSignal.WaitAsync(cancellationToken);
				try
				{
					if (_toRtgsCall is not null)
					{
						await _toRtgsCall.RequestStream.CompleteAsync();
						_toRtgsCall.Dispose();
						_toRtgsCall = null;
					}
				}
				finally
				{
					_writingSignal.Release();
				}
			}

			_resetConnection = false;
		}

		if (_toRtgsCall is null)
		{
			var grpcCallHeaders = new Metadata { new("bankdid", _options.BankDid) };
			_toRtgsCall = _paymentClient.ToRtgsMessage(grpcCallHeaders, cancellationToken: CancellationToken.None);

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
				if (acknowledgement.CorrelationId == _acknowledgementContext?.CorrelationId)
				{
					_acknowledgementContext?.Release(acknowledgement);
				}
			}
		}
		catch (RpcException ex)
		{
			if (_acknowledgementContext is null)
			{
				_logger.LogError(ex, "RTGS connection unexpectedly closed");
			}

			_resetConnection = true;

			_acknowledgementContext?.Release(ex);
		}
	}

	private async Task SendMessageAsync<T>(
		T message, 
		string messageIdentifier, 
		IDictionary<string, string> headers, 
		string callingMethod, 
		string idCryptAlias,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var rtgsMessage = new RtgsMessage
		{
			Data = JsonSerializer.Serialize(message),
			MessageIdentifier = messageIdentifier,
			CorrelationId = _acknowledgementContext.CorrelationId
		};

		if (headers != null)
		{
			rtgsMessage.Headers.Add(headers);
		}

		await SignMessageAsync(message, idCryptAlias, rtgsMessage.Headers);

		await _writingSignal.WaitAsync(cancellationToken);

		try
		{
			_logger.LogInformation("Sending {MessageType} to RTGS ({CallingMethod})", typeof(T).Name, callingMethod);

			await _toRtgsCall.RequestStream.WriteAsync(rtgsMessage);

			_logger.LogInformation("Sent {MessageType} to RTGS ({CallingMethod})", typeof(T).Name, callingMethod);
		}
		catch (RpcException)
		{
			_resetConnection = true;
			throw;
		}
		finally
		{
			_writingSignal.Release();
		}
	}

	private void LogAcknowledgementResult<T>(string callingMethod)
	{
		if (_acknowledgementContext.RpcException is not null)
		{
			_logger.LogError(_acknowledgementContext.RpcException, "Error received when sending {MessageType} to RTGS ({CallingMethod})", typeof(T).Name, callingMethod);

			return;
		}

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

			case SendResult.Unknown:
			default:
				_logger.LogWarning("Received unexpected {MessageType} acknowledgement ({Status}) from RTGS ({CallingMethod})", typeof(T).Name, _acknowledgementContext.Status, callingMethod);
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
				await _writingSignal.WaitAsync();
				try
				{
					if (_toRtgsCall is not null)
					{
						await _toRtgsCall.RequestStream.CompleteAsync();

						await _waitForAcknowledgementsTask;

						_toRtgsCall.Dispose();
						_toRtgsCall = null;
					}
				}
				finally
				{
					_writingSignal.Release();
				}
			}

			_acknowledgementContext?.Dispose();
			_sharedTokenSource.Dispose();
		}
		finally
		{
			_disposingSignal.Release();
		}
	}

	private sealed class AcknowledgementContext : IDisposable
	{
		private SemaphoreSlim _acknowledgementSignal;
		private bool _handled;

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
			cancellationToken.ThrowIfCancellationRequested();

			if (_handled)
			{
				return;
			}

			var enteredSemaphore = await _acknowledgementSignal.WaitAsync(timeout, cancellationToken);
			if (!enteredSemaphore)
			{
				_handled = true;
				Status = SendResult.Timeout;
			}
		}

		public void Release(RtgsMessageAcknowledgement acknowledgement)
		{
			if (_handled)
			{
				return;
			}

			_handled = true;
			Status = acknowledgement.Success ? SendResult.Success : SendResult.Rejected;

			_acknowledgementSignal?.Release();
		}

		public void Release(RpcException exception)
		{
			if (_handled)
			{
				return;
			}

			_handled = true;
			RpcException = exception;

			_acknowledgementSignal?.Release();
		}

		public void Dispose()
		{
			_acknowledgementSignal?.Dispose();
			_acknowledgementSignal = null;
		}
	}
}
