extern alias RTGSServer;
using RTGSServer::RTGS.Public.Payment.V2;
using System.Collections.Generic;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer
{
	public class ToRtgsReceiver
	{
		private readonly List<RtgsMessage> _requests = new();

		public IEnumerable<RtgsMessage> Requests =>
			_requests;

		public void AddRequest(RtgsMessage request) =>
			_requests.Add(request);
	}
}
