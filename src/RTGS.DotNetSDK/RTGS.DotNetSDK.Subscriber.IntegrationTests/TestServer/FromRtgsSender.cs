extern alias RTGSServer;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using RTGSServer::RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestServer
{
	public class FromRtgsSender
	{
		private static readonly TimeSpan WaitForReadyToSendDuration = TimeSpan.FromSeconds(3);

		private readonly ManualResetEventSlim _readyToSend = new(false);
		private readonly List<RtgsMessageAcknowledgement> _acknowledgements = new();
		private readonly CountdownEvent _acknowledgementsSignal = new(1);
		private IServerStreamWriter<RtgsMessage> _messageStream;

		public IEnumerable<RtgsMessageAcknowledgement> Acknowledgements => _acknowledgements;

		public void Register(IServerStreamWriter<RtgsMessage> messageStream)
		{
			_messageStream = messageStream;
			_readyToSend.Set();
		}

		public void Unregister()
		{
			_readyToSend.Reset();
			_messageStream = null;
		}

		public async Task<RtgsMessage> SendAsync<T>(string instructionType, T data)
		{
			_readyToSend.Wait(WaitForReadyToSendDuration);

			if (_messageStream is null)
			{
				return null;
			}

			var correlationId = Guid.NewGuid().ToString();

			var rtgsMessage = new RtgsMessage
			{
				Header = new RtgsMessageHeader
				{
					CorrelationId = correlationId,
					InstructionType = instructionType
				},
				Data = JsonSerializer.Serialize(data)
			};

			await _messageStream.WriteAsync(rtgsMessage);

			return rtgsMessage;
		}

		public void SetExpectedNumberOfAcknowledgements(int count) =>
			_acknowledgementsSignal.Reset(count);

		public void AddAcknowledgement(RtgsMessageAcknowledgement acknowledgement)
		{
			_acknowledgements.Add(acknowledgement);
			_acknowledgementsSignal.Signal();
		}

		public void WaitForAcknowledgements() =>
			_acknowledgementsSignal.Wait();

		public void Clear()
		{
			Unregister();
			_acknowledgements.Clear();
		}
	}
}
