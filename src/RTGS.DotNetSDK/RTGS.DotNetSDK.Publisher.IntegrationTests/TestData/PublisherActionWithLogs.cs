using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Publisher.IntegrationTests.Logging;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public class PublisherActionWithLogs<TRequest> : IPublisherAction<TRequest>
	{
		private readonly IPublisherAction<TRequest> _publisherAction;

		public PublisherActionWithLogs(IPublisherAction<TRequest> publisherAction, IReadOnlyList<LogEntry> informationLogs)
		{
			_publisherAction = publisherAction;
			InformationLogs = informationLogs;
		}

		public TRequest Request => _publisherAction.Request;

		public Task<SendResult> InvokeSendDelegateAsync(IRtgsPublisher publisher, CancellationToken cancellationToken = default) =>
			_publisherAction.InvokeSendDelegateAsync(publisher, cancellationToken);

		public IReadOnlyList<LogEntry> InformationLogs { get; }
	}
}
