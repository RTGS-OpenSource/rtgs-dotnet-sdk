namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public class SubscriberActionWithLogs<TMessage> : ISubscriberAction<TMessage>
{
	private readonly SubscriberAction<TMessage> _subscriberAction;
	private readonly IReadOnlyList<LogEntry> _logs;

	public SubscriberActionWithLogs(SubscriberAction<TMessage> subscriberAction, IReadOnlyList<LogEntry> logs)
	{
		_subscriberAction = subscriberAction;
		_logs = logs;
	}

	public string MessageIdentifier => _subscriberAction.MessageIdentifier;

	public TMessage Message => _subscriberAction.Message;

	public Dictionary<string, string> AdditionalHeaders => _subscriberAction.AdditionalHeaders;

	public IEnumerable<LogEntry> SubscriberLogs(LogEventLevel logLevel) =>
		_logs.Where(log => log.LogLevel == logLevel);
}
