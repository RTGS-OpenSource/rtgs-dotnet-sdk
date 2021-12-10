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
		private static readonly TimeSpan WaitForReadyToSendDuration = TimeSpan.FromSeconds(1);

		private readonly ManualResetEventSlim _readyToSend = new(false);
		private readonly List<RtgsMessageAcknowledgement> _acknowledgements = new();
		private readonly CountdownEvent _acknowledgementsSignal = new(1);
		private IServerStreamWriter<RtgsMessage> _messageStream;

		public IEnumerable<RtgsMessageAcknowledgement> Acknowledgements => _acknowledgements;

		public Metadata RequestHeaders { get; private set; }

		public void Register(IServerStreamWriter<RtgsMessage> messageStream, Metadata requestHeaders)
		{
			_messageStream = messageStream;
			RequestHeaders = requestHeaders;
			_readyToSend.Set();
		}

		public void Unregister()
		{
			_readyToSend.Reset();
			_messageStream = null;
			RequestHeaders = null;
		}

		public async Task<RtgsMessage> SendAsync<T>(string messageIdentifier, T data)
		{
			var messageStreamSet = _readyToSend.Wait(WaitForReadyToSendDuration);
			if (!messageStreamSet)
			{
				return null;
			}

			if (_messageStream is null)
			{
				throw new InvalidOperationException("message stream not set");
			}

			var correlationId = Guid.NewGuid().ToString();

			var rtgsMessage = new RtgsMessage
			{
				Header = new RtgsMessageHeader
				{
					CorrelationId = correlationId,
					InstructionType = messageIdentifier
				},
				Data = JsonSerializer.Serialize(data)
			};

			await _messageStream.WriteAsync(rtgsMessage);

			return rtgsMessage;
		}

		public void AddAcknowledgement(RtgsMessageAcknowledgement acknowledgement)
		{
			_acknowledgements.Add(acknowledgement);
			_acknowledgementsSignal.Signal();
		}

		public void WaitForAcknowledgements(TimeSpan timeout) =>
			_acknowledgementsSignal.Wait(timeout);

		public void Reset()
		{
			Unregister();
			_acknowledgements.Clear();
			_acknowledgementsSignal.Reset(1);
		}
	}
}
