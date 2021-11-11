extern alias RTGSServer;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RTGSServer::RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer
{
	public class ToRtgsMessageHandler
	{
		private Func<RtgsMessage, RtgsMessageAcknowledgement> _generateAcknowledgement;

		public ToRtgsMessageHandler()
		{
			ReturnAcknowledgementWithSuccess();
		}

		public async Task Handle(RtgsMessage message, IServerStreamWriter<RtgsMessageAcknowledgement> responseStream) =>
			await responseStream.WriteAsync(_generateAcknowledgement(message));

		public void ReturnAcknowledgementWithFailure() =>
			_generateAcknowledgement = message => new RtgsMessageAcknowledgement
			{
				Code = (int)StatusCode.Internal,
				Success = false,
				Header = message.Header
			};

		public void ReturnAcknowledgementWithSuccess() =>
			_generateAcknowledgement = message => new RtgsMessageAcknowledgement
			{
				Code = (int)StatusCode.OK,
				Success = true,
				Header = message.Header
			};
	}
}
