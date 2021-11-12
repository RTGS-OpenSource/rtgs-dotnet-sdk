extern alias RTGSServer;
using System;
using System.Threading.Tasks;
using Grpc.Core;
using RTGSServer::RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer
{
	public class ToRtgsMessageHandler
	{
		private Func<RtgsMessage, Task<RtgsMessageAcknowledgement[]>> _generateAcknowledgements;

		public ToRtgsMessageHandler()
		{
			ReturnAcknowledgementWithSuccess();
		}

		public async Task Handle(RtgsMessage message, IServerStreamWriter<RtgsMessageAcknowledgement> responseStream)
		{
			var acknowledgements = await _generateAcknowledgements(message);

			foreach (var acknowledgement in acknowledgements)
			{
				await responseStream.WriteAsync(acknowledgement);
			}
		}

		public void ReturnAcknowledgementWithFailure() =>
			_generateAcknowledgements = message => Task.FromResult(new[]
			{
				new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.Internal,
					Success = false,
					Header = message.Header
				}
			});

		public void ReturnAcknowledgementWithSuccess() =>
			_generateAcknowledgements = message => Task.FromResult(new[]
			{
				new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.OK,
					Success = true,
					Header = message.Header
				}
			});

		public void ReturnAcknowledgementTooLate(TimeSpan timeSpan) =>
			_generateAcknowledgements = async message =>
			{
				await Task.Delay(timeSpan);

				return new[]
				{
					new RtgsMessageAcknowledgement
					{
						Code = (int)StatusCode.OK,
						Success = true,
						Header = message.Header
					}
				};
			};

		public void ReturnUnexpectedSuccessfulAcknowledgement() =>
			_generateAcknowledgements = message => Task.FromResult(new[]
			{
				new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.OK,
					Success = true,
					Header = new RtgsMessageHeader
					{
						CorrelationId = Guid.NewGuid().ToString(),
						InstructionType = message.Header.InstructionType
					}
				}
			});

		public void ReturnUnexpectedSuccessfulAcknowledgementThenAcknowledgementWithFailure() =>
			_generateAcknowledgements = message => Task.FromResult(new[]
			{
				new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.OK,
					Success = true,
					Header = new RtgsMessageHeader
					{
						CorrelationId = Guid.NewGuid().ToString(),
						InstructionType = message.Header.InstructionType
					}
				},
				new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.Internal,
					Success = false,
					Header = message.Header
				}
			});

		public void ReturnAcknowledgementWithFailureThenUnexpectedSuccessfulAcknowledgement() =>
			_generateAcknowledgements = message => Task.FromResult(new[]
			{
				new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.OK,
					Success = true,
					Header = new RtgsMessageHeader
					{
						CorrelationId = Guid.NewGuid().ToString(),
						InstructionType = message.Header.InstructionType
					}
				},
				new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.Internal,
					Success = false,
					Header = message.Header
				}
			});

		public void ReturnAcknowledgementWithSuccessBeforeAndAfterUnexpectedFailures() =>
			_generateAcknowledgements = message => Task.FromResult(new[]
			{
				new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.Internal,
					Success = false,
					Header = new RtgsMessageHeader
					{
						CorrelationId = Guid.NewGuid().ToString(),
						InstructionType = message.Header.InstructionType
					}
				},
				new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.OK,
					Success = true,
					Header = message.Header
				},
				new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.Internal,
					Success = false,
					Header = new RtgsMessageHeader
					{
						CorrelationId = Guid.NewGuid().ToString(),
						InstructionType = message.Header.InstructionType
					}
				}
			});
	}
}
