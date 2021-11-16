extern alias RTGSServer;
using Grpc.Core;
using RTGSServer::RTGS.Public.Payment.V2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer
{
	public class ToRtgsMessageHandler
	{
		private readonly Queue<Func<RtgsMessage, Task<RtgsMessageAcknowledgement>>> _generateAcknowledgements = new();

		public async Task Handle(RtgsMessage message, IServerStreamWriter<RtgsMessageAcknowledgement> responseStream)
		{
			while (_generateAcknowledgements.TryDequeue(out var generateAcknowledgement))
			{
				var acknowledgement = await generateAcknowledgement(message);
				await responseStream.WriteAsync(acknowledgement);
			}
		}

		public void Clear() => _generateAcknowledgements.Clear();

		public void EnqueueExpectedAcknowledgementWithFailure() =>
			EnqueueAcknowledgementWithFailure(true);

		public void EnqueueUnexpectedAcknowledgementWithFailure() =>
			EnqueueAcknowledgementWithFailure(false);

		private void EnqueueAcknowledgementWithFailure(bool expected) =>
			_generateAcknowledgements.Enqueue(message => Task.FromResult(
				new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.Internal,
					Success = false,
					Header = expected
						? message.Header
						: GenerateUnexpectedMessageHeader(message.Header)
				}));

		public void EnqueueExpectedAcknowledgementWithSuccess() =>
			EnqueueAcknowledgementWithSuccess(true);

		public void EnqueueUnexpectedAcknowledgementWithSuccess() =>
			EnqueueAcknowledgementWithSuccess(false);

		private void EnqueueAcknowledgementWithSuccess(bool expected) =>
			_generateAcknowledgements.Enqueue(message => Task.FromResult(
				new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.OK,
					Success = true,
					Header = expected
						? message.Header
						: GenerateUnexpectedMessageHeader(message.Header)
				}));

		public void EnqueueExpectedAcknowledgementWithDelay(TimeSpan timeSpan) =>
			_generateAcknowledgements.Enqueue(async message =>
			{
				await Task.Delay(timeSpan);

				return new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.OK,
					Success = true,
					Header = message.Header
				};
			});

		private static RtgsMessageHeader GenerateUnexpectedMessageHeader(RtgsMessageHeader original) =>
			new()
			{
				CorrelationId = Guid.NewGuid().ToString(),
				InstructionType = original.InstructionType
			};
	}
}
