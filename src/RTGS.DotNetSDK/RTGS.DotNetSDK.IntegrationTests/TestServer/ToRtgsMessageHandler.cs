extern alias RTGSServer;
using System.Collections.Concurrent;
using RTGSServer::RTGS.Public.Payment.V4;

namespace RTGS.DotNetSDK.IntegrationTests.TestServer;

public class ToRtgsMessageHandler
{
	private readonly ConcurrentQueue<IEnumerable<Func<RtgsMessage, Task<RtgsMessageAcknowledgement>>>> _acknowledgementGeneratorsQueue = new();

	public async Task Handle(RtgsMessage message, IServerStreamWriter<RtgsMessageAcknowledgement> responseStream)
	{
		if (_acknowledgementGeneratorsQueue.TryDequeue(out var acknowledgementGenerators))
		{
			foreach (var acknowledgementGenerator in acknowledgementGenerators)
			{
				var acknowledgement = await acknowledgementGenerator(message);
				await responseStream.WriteAsync(acknowledgement);
			}
		}
	}

	public void SetupForMessage(Action<ISetupForMessageOptions> configure)
	{
		var options = new SetupForMessageOptions();
		configure(options);

		_acknowledgementGeneratorsQueue.Enqueue(options.GenerateAcknowledgements);
	}

	public void Clear() => _acknowledgementGeneratorsQueue.Clear();

	private class SetupForMessageOptions : ISetupForMessageOptions
	{
		public List<Func<RtgsMessage, Task<RtgsMessageAcknowledgement>>> GenerateAcknowledgements { get; } = new();

		public void ReturnExpectedAcknowledgementWithFailure() =>
			ReturnAcknowledgementWithFailure(true);

		public void ReturnUnexpectedAcknowledgementWithFailure() =>
			ReturnAcknowledgementWithFailure(false);

		private void ReturnAcknowledgementWithFailure(bool expected) =>
			GenerateAcknowledgements.Add(message => Task.FromResult(
				new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.Internal,
					Success = false,
					CorrelationId = expected ? message.CorrelationId : Guid.NewGuid().ToString()
				}));

		public void ReturnExpectedAcknowledgementWithSuccess() =>
			ReturnAcknowledgementWithSuccess(true);

		public void ReturnUnexpectedAcknowledgementWithSuccess() =>
			ReturnAcknowledgementWithSuccess(false);

		private void ReturnAcknowledgementWithSuccess(bool expected) =>
			GenerateAcknowledgements.Add(message => Task.FromResult(
				new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.OK,
					Success = true,
					CorrelationId = expected ? message.CorrelationId : Guid.NewGuid().ToString()
				}));

		public void ReturnExpectedAcknowledgementWithDelay(TimeSpan timeSpan) =>
			GenerateAcknowledgements.Add(async message =>
			{
				await Task.Delay(timeSpan);

				return new RtgsMessageAcknowledgement
				{
					Code = (int)StatusCode.OK,
					Success = true,
					CorrelationId = message.CorrelationId
				};
			});

		public void ThrowRpcException(StatusCode statusCode, string detail) =>
			GenerateAcknowledgements.Add(_ => throw new RpcException(new Status(statusCode, detail)));
	}

	public interface ISetupForMessageOptions
	{
		void ReturnExpectedAcknowledgementWithFailure();
		void ReturnUnexpectedAcknowledgementWithFailure();
		void ReturnExpectedAcknowledgementWithSuccess();
		void ReturnUnexpectedAcknowledgementWithSuccess();
		void ReturnExpectedAcknowledgementWithDelay(TimeSpan timeSpan);
		void ThrowRpcException(StatusCode statusCode, string detail);
	}
}
