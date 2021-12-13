using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.Subscriber.Exceptions;
using RTGS.DotNetSDK.Subscriber.HandleMessageCommands;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber
{
	internal sealed class RtgsSubscriber : IRtgsSubscriber
	{
		private readonly ILogger<RtgsSubscriber> _logger;
		private readonly Payment.PaymentClient _grpcClient;
		private readonly RtgsSubscriberOptions _options;
		private readonly IHandleMessageCommandsFactory _handleMessageCommandsFactory;
		private readonly SemaphoreSlim _disposingSignal = new(1);
		private Task _executingTask;
		private AsyncDuplexStreamingCall<RtgsMessageAcknowledgement, RtgsMessage> _fromRtgsCall;
		private bool _disposed;

		public event EventHandler<ExceptionEventArgs> OnExceptionOccurred;

		public RtgsSubscriber(
			ILogger<RtgsSubscriber> logger,
			Payment.PaymentClient grpcClient,
			RtgsSubscriberOptions options,
			IHandleMessageCommandsFactory handleMessageCommandsFactory)
		{
			_logger = logger;
			_grpcClient = grpcClient;
			_options = options;
			_handleMessageCommandsFactory = handleMessageCommandsFactory;
		}

		public void Start(IEnumerable<IHandler> handlers)
		{
			// TODO: thread safety?
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(RtgsSubscriber));
			}

			if (_executingTask is not null)
			{
				throw new InvalidOperationException("RTGS Subscriber is already running");
			}

			_executingTask = Execute(handlers.ToList());
		}

		private async Task Execute(IReadOnlyCollection<IHandler> handlers)
		{
			_logger.LogInformation("RTGS Subscriber started");

			try
			{
				var commands = _handleMessageCommandsFactory.CreateAll(handlers)
					.ToDictionary(command => command.MessageIdentifier, command => command);

				var grpcCallHeaders = new Metadata { new("bankdid", _options.BankDid) };
				_fromRtgsCall = _grpcClient.FromRtgsMessage(grpcCallHeaders);

				await foreach (var rtgsMessage in _fromRtgsCall.ResponseStream.ReadAllAsync())
				{
					await ProcessRtgsMessage(commands, rtgsMessage);
				}
			}
			catch (RpcException ex)
			{
				_logger.LogError(ex, "An error occurred while communicating with RTGS");

				RaiseExceptionOccurredEvent(ex);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An unknown error occurred");

				RaiseExceptionOccurredEvent(ex);
			}
		}

		private async Task ProcessRtgsMessage(IReadOnlyDictionary<string, IHandleMessageCommand> commands, RtgsMessage rtgsMessage)
		{
			try
			{
				if (rtgsMessage.Header is null)
				{
					await SendFailureAcknowledgement(rtgsMessage.Header);
					throw new RtgsSubscriberException("Message with no header received");
				}

				if (string.IsNullOrWhiteSpace(rtgsMessage.Header.InstructionType))
				{
					await SendFailureAcknowledgement(rtgsMessage.Header);
					throw new RtgsSubscriberException("Message with no identifier received");
				}

				_logger.LogInformation("{MessageIdentifier} message received from RTGS", rtgsMessage.Header.InstructionType);

				if (!commands.TryGetValue(rtgsMessage.Header.InstructionType, out var command))
				{
					await SendFailureAcknowledgement(rtgsMessage.Header);
					throw new RtgsSubscriberException("No handler found for message", rtgsMessage.Header.InstructionType);
				}

				// We need to send back the acknowledgement as soon as possible to avoid timeouts on the server.
				// The handler should be quick but we cannot guarentee that is the case so do this first.
				await SendSuccessAcknowledgement(rtgsMessage.Header);

				try
				{
					await command.HandleAsync(rtgsMessage);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "An error occurred while handling a message (MessageIdentifier: {MessageIdentifier})", command.MessageIdentifier);

					RaiseExceptionOccurredEvent(ex);
				}
			}
			catch (RtgsSubscriberException ex)
			{
				_logger.LogError(ex, "An error occurred while processing a message (MessageIdentifier: {MessageIdentifier})", ex.MessageIdentifier);

				RaiseExceptionOccurredEvent(ex);
			}
		}

		private void RaiseExceptionOccurredEvent(Exception raisedException)
		{
			try
			{
				var eventHandler = OnExceptionOccurred;
				eventHandler?.Invoke(this, new ExceptionEventArgs(raisedException));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while raising exception occurred event");
			}
		}

		private Task SendSuccessAcknowledgement(RtgsMessageHeader header) =>
			SendAcknowledgement(header, true);

		private Task SendFailureAcknowledgement(RtgsMessageHeader header) =>
			SendAcknowledgement(header, false);

		private async Task SendAcknowledgement(RtgsMessageHeader header, bool success)
		{
			var acknowledgement = new RtgsMessageAcknowledgement
			{
				Header = header ?? new RtgsMessageHeader(),
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

			if (_executingTask is null)
			{
				throw new InvalidOperationException("RTGS Subscriber is not running");
			}

			_logger.LogInformation("RTGS Subscriber stopping");

			await CompleteAsyncEnumerables();
			_executingTask = null;

			_logger.LogInformation("RTGS Subscriber stopped");
		}

		private async Task CompleteAsyncEnumerables()
		{
			// TODO: can't stop if processing
			if (_fromRtgsCall is not null)
			{
				await _fromRtgsCall.RequestStream.CompleteAsync();

				await _executingTask;

				_fromRtgsCall.Dispose();
				_fromRtgsCall = null;
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
			}
			finally
			{
				_disposingSignal.Release();
			}
		}
	}
}
