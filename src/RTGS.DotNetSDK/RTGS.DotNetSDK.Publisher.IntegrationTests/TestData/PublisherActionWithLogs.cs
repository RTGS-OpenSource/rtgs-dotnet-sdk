using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RTGS.DotNetSDK.Publisher.IntegrationTests.Logging;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public class PublisherActionWithLogs<TRequest> : IPublisherAction<TRequest>
	{
		private readonly IPublisherAction<TRequest> _publisherAction;

		public PublisherActionWithLogs(IPublisherAction<TRequest> publisherAction, List<LogEntry> logs)
		{
			_publisherAction = publisherAction;
			Logs = logs;
		}

		public TRequest Request => _publisherAction.Request;

		public Task<SendResult> InvokeSendDelegateAsync(IRtgsPublisher publisher, CancellationToken cancellationToken = default) =>
			_publisherAction.InvokeSendDelegateAsync(publisher, cancellationToken);

		public List<LogEntry> Logs { get; }
	}
}
