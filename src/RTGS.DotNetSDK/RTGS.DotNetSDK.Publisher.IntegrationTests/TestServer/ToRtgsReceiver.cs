extern alias RTGSServer;
using Grpc.Core;
using RTGSServer::RTGS.Public.Payment.V2;
using System.Collections.Generic;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer
{
	public class ToRtgsReceiver
	{
		public List<ToRtgsConnectionInfo> Connections { get; } = new();

		public int NumberOfConnections => Connections.Count;

		public IList<RtgsMessage> SetupConnectionInfo(Metadata headers)
		{
			var connectionInfo = new ToRtgsConnectionInfo(headers);

			Connections.Add(connectionInfo);

			return connectionInfo.Requests;
		}

		public class ToRtgsConnectionInfo
		{
			public ToRtgsConnectionInfo(Metadata headers)
			{
				Headers = headers;
			}

			public List<RtgsMessage> Requests { get; } = new();
			public Metadata Headers { get; }
		}
	}
}
