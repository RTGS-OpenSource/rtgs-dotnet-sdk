extern alias RTGSServer;
using System.Collections.Concurrent;
using RTGSServer::RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer;

public class ToRtgsReceiver
{
	private Action _messageReceivedAction;

	public ConcurrentBag<ToRtgsConnectionInfo> Connections { get; } = new();

	public int NumberOfConnections => Connections.Count;

	public bool ThrowOnConnection { get; set; }

	public ToRtgsConnectionInfo SetupConnectionInfo(Metadata headers)
	{
		if (ThrowOnConnection)
		{
			throw new InvalidOperationException("The receiver was configured to throw on connection");
		}

		var connectionInfo = new ToRtgsConnectionInfo(headers, this);

		Connections.Add(connectionInfo);

		return connectionInfo;
	}

	public void RegisterOnMessageReceived(Action action) =>
		_messageReceivedAction = action;

	private void MessageReceived() =>
		_messageReceivedAction?.Invoke();

	public class ToRtgsConnectionInfo
	{
		private readonly ToRtgsReceiver _parent;
		private readonly List<RtgsMessage> _requests = new();

		public ToRtgsConnectionInfo(Metadata headers, ToRtgsReceiver parent)
		{
			Headers = headers;
			_parent = parent;
		}

		public IEnumerable<RtgsMessage> Requests => _requests;
		public Metadata Headers { get; }

		public void Add(RtgsMessage message)
		{
			_requests.Add(message);

			_parent.MessageReceived();
		}
	}
}
