extern alias RTGSServer;
using Grpc.Core;
using RTGSServer::RTGS.Public.Payment.V2;
using System;
using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer
{
	public class ToRtgsMessageHandler
	{
		private Func<RtgsMessage, Task<RtgsMessageAcknowledgement>> _generateAcknowledgement;

		public ToRtgsMessageHandler()
		{
			ReturnAcknowledgementWithSuccess();
		}

		public async Task Handle(RtgsMessage message, IServerStreamWriter<RtgsMessageAcknowledgement> responseStream) =>
			await responseStream.WriteAsync(await _generateAcknowledgement(message));

		public void ReturnAcknowledgementWithFailure() =>
			_generateAcknowledgement = message => Task.FromResult(new RtgsMessageAcknowledgement
			{
				Code = (int)StatusCode.Internal,
				Success = false,
				Header = message.Header
			});

		public void ReturnAcknowledgementWithSuccess() =>
			_generateAcknowledgement = message => Task.FromResult(new RtgsMessageAcknowledgement
			{
				Code = (int)StatusCode.OK,
				Success = true,
				Header = message.Header
			});

		public void ReturnAcknowledgementTooLate(TimeSpan timeSpan) =>
			_generateAcknowledgement = async message =>
			{
				await Task.Delay(timeSpan);

				return new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.OK,
					Success = true,
					Header = message.Header
				};
			};

		public void ReturnUnexpectedSuccessfulAcknowledgement() =>
			_generateAcknowledgement = message => Task.FromResult(new RtgsMessageAcknowledgement
			{
				Code = (int)StatusCode.OK,
				Success = true,
				Header = new RtgsMessageHeader
				{
					CorrelationId = Guid.NewGuid().ToString(),
					InstructionType = message.Header.InstructionType
				}
			});
	}
}
