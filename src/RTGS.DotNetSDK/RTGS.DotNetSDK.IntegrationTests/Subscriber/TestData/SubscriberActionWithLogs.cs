using RTGS.DotNetSDK.Subscriber.Handlers;

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

	public ITestHandler<TMessage> Handler => _subscriberAction.Handler;

	public string MessageIdentifier => _subscriberAction.MessageIdentifier;

	public TMessage Message => _subscriberAction.Message;

	public Dictionary<string, string> AdditionalHeaders => _subscriberAction.AdditionalHeaders;

	public IReadOnlyCollection<IHandler> AllTestHandlers => _subscriberAction.AllTestHandlers;

	public IEnumerable<LogEntry> SubscriberLogs(LogEventLevel logLevel) =>
		_logs.Where(log => log.LogLevel == logLevel);
}
