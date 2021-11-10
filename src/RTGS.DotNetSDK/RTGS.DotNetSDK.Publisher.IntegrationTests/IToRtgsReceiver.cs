extern alias RTGSServer;
using System.Collections.Generic;
using RTGSServer::RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public interface IToRtgsReceiver
	{
		bool HasRequests { get; }
		IEnumerable<RtgsMessage> Requests { get; }

		void AddRequest(RtgsMessage request);
	}
}
