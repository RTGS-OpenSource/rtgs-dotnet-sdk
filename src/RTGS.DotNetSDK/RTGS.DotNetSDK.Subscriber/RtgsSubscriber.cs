using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
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
			if (_executingTask is not null)
			{
				throw new InvalidOperationException("RTGS Subscriber has already been started");
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

				await foreach (var message in _fromRtgsCall.ResponseStream.ReadAllAsync())
				{
					// TODO: message with no header/instruction type
					_logger.LogInformation("{MessageIdentifier} message received from RTGS", message.Header.InstructionType);

					// TODO: command not found
					commands.TryGetValue(message.Header.InstructionType, out var command);

					var acknowledgement = new RtgsMessageAcknowledgement
					{
						Header = new RtgsMessageHeader
						{
							CorrelationId = message.Header.CorrelationId,
							InstructionType = message.Header.InstructionType
						},
						Success = true
					};

					await _fromRtgsCall.RequestStream.WriteAsync(acknowledgement);

					// TODO: squash exceptions?
					await command.HandleAsync(message);
				}
			}
			catch (RpcException ex)
			{
				_logger.LogError(ex, "An error occurred while communicating with RTGS");

				var eventHandler = OnExceptionOccurred;
				eventHandler?.Invoke(this, new ExceptionEventArgs(ex));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An unknown error occurred");

				var eventHandler = OnExceptionOccurred;
				eventHandler?.Invoke(this, new ExceptionEventArgs(ex));
			}
		}

		// TODO: what if called without calling start?
		public async Task StopAsync()
		{
			_logger.LogInformation("RTGS Subscriber stopping");

			await CompleteAsyncEnumerables();

			_logger.LogInformation("RTGS Subscriber stopped");
		}

		private async Task CompleteAsyncEnumerables()
		{
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

				await StopAsync();
			}
			finally
			{
				_disposingSignal.Release();
			}
		}
	}
}
