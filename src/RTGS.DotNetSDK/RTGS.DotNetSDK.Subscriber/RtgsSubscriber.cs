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
		private readonly IHandleMessageCommandsFactory _handleMessageCommandsFactory;
		private readonly SemaphoreSlim _disposingSignal = new(1);
		private Task _executingTask;
		private AsyncDuplexStreamingCall<RtgsMessageAcknowledgement, RtgsMessage> _fromRtgsCall;
		private bool _disposed;

		public RtgsSubscriber(
			ILogger<RtgsSubscriber> logger, 
			Payment.PaymentClient grpcClient, 
			IHandleMessageCommandsFactory handleMessageCommandsFactory)
		{
			_logger = logger;
			_grpcClient = grpcClient;
			_handleMessageCommandsFactory = handleMessageCommandsFactory;
		}

		// TODO: what if called twice?
		public void Start(IEnumerable<IHandler> handlers) =>
			_executingTask = Execute(handlers.ToList());

		private async Task Execute(IReadOnlyCollection<IHandler> handlers)
		{
			_logger.LogInformation("RTGS Subscriber started");

			_fromRtgsCall = _grpcClient.FromRtgsMessage();

			var commands = _handleMessageCommandsFactory.CreateAll(handlers)
				.ToDictionary(command => command.MessageIdentifier, command => command);

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
