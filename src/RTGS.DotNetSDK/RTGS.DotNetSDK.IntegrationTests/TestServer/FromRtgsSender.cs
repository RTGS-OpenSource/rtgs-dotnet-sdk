extern alias RTGSServer;
using System.Text.Json;
using RTGSServer::RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.IntegrationTests.TestServer;

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

	public bool WaitForConnection() =>
		_readyToSend.Wait(WaitForReadyToSendDuration);

	public async Task<RtgsMessage> SendAsync<T>(string messageIdentifier, T data, Dictionary<string, string> additionalHeaders = null, Action<RtgsMessage> customiseRtgsMessage = null)
	{
		var messageStreamSet = WaitForConnection();
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
			CorrelationId = correlationId,
			MessageIdentifier = messageIdentifier,
			Data = JsonSerializer.Serialize(data)
		};

		if (additionalHeaders is not null && additionalHeaders.Any())
		{
			rtgsMessage.Headers.Add(additionalHeaders);
		}

		if (customiseRtgsMessage is not null)
		{
			customiseRtgsMessage(rtgsMessage);
		}

		await _messageStream.WriteAsync(rtgsMessage);

		return rtgsMessage;
	}

	public void SetExpectedAcknowledgementCount(int count) =>
		_acknowledgementsSignal.Reset(count);

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
