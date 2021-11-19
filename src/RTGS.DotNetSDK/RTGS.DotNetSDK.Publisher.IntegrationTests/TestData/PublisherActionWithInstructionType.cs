using System.Threading;
using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public class PublisherActionWithInstructionType<TRequest> : IPublisherAction<TRequest>
	{
		private readonly IPublisherAction<TRequest> _publisherAction;

		public PublisherActionWithInstructionType(IPublisherAction<TRequest> publisherAction, string instructionType)
		{
			_publisherAction = publisherAction;
			InstructionType = instructionType;
		}

		public TRequest Request => _publisherAction.Request;

		public Task<SendResult> InvokeSendDelegateAsync(IRtgsPublisher publisher, CancellationToken cancellationToken = default) =>
			_publisherAction.InvokeSendDelegateAsync(publisher, cancellationToken);

		public string InstructionType { get; }
	}
}
