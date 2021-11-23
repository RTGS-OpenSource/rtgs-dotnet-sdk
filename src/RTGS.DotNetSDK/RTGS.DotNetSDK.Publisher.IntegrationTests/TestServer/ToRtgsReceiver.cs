﻿extern alias RTGSServer;
using System;
using System.Collections.Generic;
using Grpc.Core;
using RTGSServer::RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer
{
	public class ToRtgsReceiver
	{
		private Action _action;

		public List<ToRtgsConnectionInfo> Connections { get; } = new();

		public int NumberOfConnections => Connections.Count;

		public ToRtgsConnectionInfo SetupConnectionInfo(Metadata headers)
		{
			var connectionInfo = new ToRtgsConnectionInfo(headers, this);

			Connections.Add(connectionInfo);

			return connectionInfo;
		}

		public void RegisterOnMessageReceived(Action action) =>
			_action = action;

		private void MessageReceived() =>
			_action?.Invoke();

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
}
