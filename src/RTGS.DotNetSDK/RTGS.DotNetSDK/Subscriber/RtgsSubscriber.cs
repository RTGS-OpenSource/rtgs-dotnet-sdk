using Grpc.Core;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Subscriber.Exceptions;
using RTGS.DotNetSDK.Subscriber.HandleMessageCommands;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Validators;
using RTGS.Public.Payment.V4;

namespace RTGS.DotNetSDK.Subscriber;

internal sealed class RtgsSubscriber : IRtgsSubscriber
{
	private readonly ILogger<RtgsSubscriber> _logger;
	private readonly Payment.PaymentClient _grpcClient;
	private readonly RtgsSdkOptions _options;
	private readonly IHandlerValidator _handlerValidator;
	private readonly IHandleMessageCommandsFactory _handleMessageCommandsFactory;

	private readonly SemaphoreSlim _startStopSignal = new(1);
	private readonly SemaphoreSlim _disposingSignal = new(1);
	private readonly SemaphoreSlim _processingSignal = new(1);
	private Task _executingTask;
	private AsyncDuplexStreamingCall<RtgsMessageAcknowledgement, RtgsMessage> _fromRtgsCall;
	private bool _disposed;
	private bool _isStopRequested;

	public bool IsRunning { get; private set; }

	public event EventHandler<ExceptionEventArgs> OnExceptionOccurred;

	public RtgsSubscriber(
		ILogger<RtgsSubscriber> logger,
		Payment.PaymentClient grpcClient,
		RtgsSdkOptions options,
		IHandlerValidator handlerValidator,
		IHandleMessageCommandsFactory handleMessageCommandsFactory)
	{
		_logger = logger;
		_grpcClient = grpcClient;
		_options = options;
		_handlerValidator = handlerValidator;
		_handleMessageCommandsFactory = handleMessageCommandsFactory;
	}

	public async Task StartAsync(IEnumerable<IHandler> handlers)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(RtgsSubscriber));
		}

		ArgumentNullException.ThrowIfNull(handlers, nameof(handlers));

		await _startStopSignal.WaitAsync();

		try
		{
			if (_executingTask is not null && _executingTask.Status != TaskStatus.RanToCompletion)
			{
				throw new InvalidOperationException("RTGS Subscriber is already running");
			}

			_isStopRequested = false;

			var handlersList = handlers.ToList();
			_handlerValidator.Validate(handlersList);

			_executingTask = Execute(handlersList);
		}
		finally
		{
			_startStopSignal.Release();
		}
	}

	private async Task Execute(IReadOnlyCollection<IHandler> handlers)
	{
		IsRunning = true;

		_logger.LogInformation("RTGS Subscriber started");

		try
		{
			var commands = _handleMessageCommandsFactory.CreateAll(handlers)
				.ToDictionary(command => command.MessageIdentifier, command => command);

			_fromRtgsCall?.Dispose();

			var grpcCallHeaders = new Metadata { new("rtgs-global-id", _options.RtgsGlobalId) };
			_fromRtgsCall = _grpcClient.FromRtgsMessage(grpcCallHeaders);

			await foreach (var rtgsMessage in _fromRtgsCall.ResponseStream.ReadAllAsync())
			{
				if (_isStopRequested)
				{
					// If the subscriber is stopping the request stream will have been completed.
					// That means it is not possible to send an acknowledgement back.
					return;
				}

				await _processingSignal.WaitAsync();

				try
				{
					await ProcessRtgsMessage(commands, rtgsMessage);
				}
				finally
				{
					_processingSignal.Release();
				}
			}

			IsRunning = false;

			if (!_isStopRequested)
			{
				var ex = new RtgsSubscriberException("Call completed unexpectedly");
				_logger.LogError(ex, "The subscriber was not stopped but the call was unexpectedly completed");
				RaiseFatalExceptionOccurredEvent(ex);
			}
		}
		catch (RpcException ex)
		{
			IsRunning = false;
			_logger.LogError(ex, "An error occurred while communicating with RTGS");

			RaiseFatalExceptionOccurredEvent(ex);
		}
		catch (Exception ex)
		{
			IsRunning = false;
			_logger.LogError(ex, "An unknown error occurred");

			RaiseFatalExceptionOccurredEvent(ex);
		}
	}

	private async Task ProcessRtgsMessage(IReadOnlyDictionary<string, IHandleMessageCommand> commands, RtgsMessage rtgsMessage)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(rtgsMessage.MessageIdentifier))
			{
				await SendFailureAcknowledgement(rtgsMessage.CorrelationId);
				throw new RtgsSubscriberException("Message with no identifier received");
			}

			_logger.LogInformation("{MessageIdentifier} message received from RTGS", rtgsMessage.MessageIdentifier);

			if (!commands.TryGetValue(rtgsMessage.MessageIdentifier, out var command))
			{
				await SendFailureAcknowledgement(rtgsMessage.CorrelationId);
				throw new RtgsSubscriberException("No handler found for message", rtgsMessage.MessageIdentifier);
			}

			// We need to send back the acknowledgement as soon as possible to avoid timeouts on the server.
			// The handler should be quick but we cannot guarantee that is the case so do this first.
			await SendSuccessAcknowledgement(rtgsMessage.CorrelationId);

			try
			{
				await command.HandleAsync(rtgsMessage);
			}
			catch (VerificationFailedException ex)
			{
				_logger.LogError(ex, "An error occurred while verifying a message (MessageIdentifier: {MessageIdentifier})", command.MessageIdentifier);
				RaiseNonFatalExceptionOccurredEvent(ex);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while handling a message (MessageIdentifier: {MessageIdentifier})", command.MessageIdentifier);

				RaiseNonFatalExceptionOccurredEvent(ex);
			}
		}
		catch (RtgsSubscriberException ex)
		{
			_logger.LogError(ex, "An error occurred while processing a message (MessageIdentifier: {MessageIdentifier})", ex.MessageIdentifier);

			RaiseNonFatalExceptionOccurredEvent(ex);
		}
	}

	private void RaiseFatalExceptionOccurredEvent(Exception raisedException)
		=> RaiseExceptionOccurredEvent(raisedException, true);

	private void RaiseNonFatalExceptionOccurredEvent(Exception raisedException)
		=> RaiseExceptionOccurredEvent(raisedException, false);

	private void RaiseExceptionOccurredEvent(Exception raisedException, bool isFatal)
	{
		try
		{
			var eventHandler = OnExceptionOccurred;
			eventHandler?.Invoke(this, new ExceptionEventArgs(raisedException, isFatal));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while raising exception occurred event");
		}
	}

	private Task SendSuccessAcknowledgement(string correlationId) =>
		SendAcknowledgement(correlationId, true);

	private Task SendFailureAcknowledgement(string correlationId) =>
		SendAcknowledgement(correlationId, false);

	private async Task SendAcknowledgement(string correlationId, bool success)
	{
		var acknowledgement = new RtgsMessageAcknowledgement
		{
			CorrelationId = correlationId,
			Success = success
		};

		await _fromRtgsCall.RequestStream.WriteAsync(acknowledgement);
	}

	public Task StopAsync() =>
		Stop(true);

	private async Task Stop(bool checkIfDisposed)
	{
		if (checkIfDisposed && _disposed)
		{
			throw new ObjectDisposedException(nameof(RtgsSubscriber));
		}

		await _startStopSignal.WaitAsync();

		try
		{
			_logger.LogInformation("RTGS Subscriber stopping");

			_isStopRequested = true;
			await CompleteAsyncEnumerables();
			_executingTask = null;
			IsRunning = false;

			_logger.LogInformation("RTGS Subscriber stopped");
		}
		finally
		{
			_startStopSignal.Release();
		}
	}

	private async Task CompleteAsyncEnumerables()
	{
		await _processingSignal.WaitAsync();

		try
		{
			if (_fromRtgsCall is not null)
			{
				await _fromRtgsCall.RequestStream.CompleteAsync();

				await _executingTask;

				_fromRtgsCall.Dispose();
				_fromRtgsCall = null;
			}
		}
		finally
		{
			_processingSignal.Release();
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

			if (_executingTask is not null)
			{
				await Stop(false);
			}

			_startStopSignal.Dispose();
			_processingSignal.Dispose();
		}
		finally
		{
			_disposingSignal.Release();
		}
	}
}
