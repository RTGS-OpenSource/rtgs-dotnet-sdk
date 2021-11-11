extern alias RTGSServer;
using System.Collections.Generic;
using System.Linq;
using RTGSServer::RTGS.Public.Payment.V2;

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
