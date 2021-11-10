extern alias RTGSServer;
using System.Collections.Generic;
using System.Linq;
using RTGSServer::RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class ToRtgsReceiver : IToRtgsReceiver
	{
		private readonly List<RtgsMessage> _requests = new();

		public bool HasRequests =>
			_requests.Any();

		public IEnumerable<RtgsMessage> Requests =>
			_requests;

		public void AddRequest(RtgsMessage request) =>
			_requests.Add(request);
	}
}
