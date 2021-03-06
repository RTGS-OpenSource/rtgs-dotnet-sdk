namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public class PublisherActionWithLogs<TRequest> : IPublisherAction<TRequest>
{
	private readonly IPublisherAction<TRequest> _publisherAction;
	private readonly IReadOnlyList<LogEntry> _logs;

	public PublisherActionWithLogs(IPublisherAction<TRequest> publisherAction, IReadOnlyList<LogEntry> logs)
	{
		_publisherAction = publisherAction;
		_logs = logs;
	}

	public TRequest Request => _publisherAction.Request;

	public Task<SendResult> InvokeSendDelegateAsync(IRtgsPublisher publisher, CancellationToken cancellationToken = default) =>
		_publisherAction.InvokeSendDelegateAsync(publisher, cancellationToken);

	public IEnumerable<LogEntry> PublisherLogs(LogEventLevel logLevel) =>
		_logs.Where(log => log.LogLevel == logLevel);
}
