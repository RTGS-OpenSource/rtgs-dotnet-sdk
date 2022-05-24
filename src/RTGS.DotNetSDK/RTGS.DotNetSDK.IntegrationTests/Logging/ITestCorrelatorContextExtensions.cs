using Serilog.Formatting.Display;

namespace RTGS.DotNetSDK.IntegrationTests.Logging;

// ReSharper disable once InconsistentNaming
public static class ITestCorrelatorContextExtensions
{
	public static IEnumerable<LogEntry> PublisherLogs(this ITestCorrelatorContext testCorrelatorContext, LogEventLevel logEventLevel) =>
		Logs(testCorrelatorContext, "RTGS.DotNetSDK.Publisher.InternalPublisher", logEventLevel);

	public static IEnumerable<LogEntry> SubscriberLogs(this ITestCorrelatorContext testCorrelatorContext, LogEventLevel logEventLevel) =>
		Logs(testCorrelatorContext, "RTGS.DotNetSDK.Subscriber.RtgsSubscriber", logEventLevel);

	public static IEnumerable<LogEntry> ConnectionBrokerLogs(this ITestCorrelatorContext testCorrelatorContext, LogEventLevel logEventLevel) =>
		Logs(testCorrelatorContext, "RTGS.DotNetSDK.Publisher.IdCrypt.RtgsConnectionBroker", logEventLevel);

	public static IEnumerable<LogEntry> LogsFor(this ITestCorrelatorContext testCorrelatorContext, string sourceContext, LogEventLevel logEventLevel) =>
		Logs(testCorrelatorContext, sourceContext, logEventLevel);

	public static IEnumerable<LogEntry> LogsForNamespace(this ITestCorrelatorContext testCorrelatorContext, string @namespace, LogEventLevel logEventLevel) =>
		Logs(testCorrelatorContext, logEventLevel, logEvent => GetSourceContext(logEvent).StartsWith(@namespace));

	private static IEnumerable<LogEntry> Logs(ITestCorrelatorContext testCorrelatorContext, string sourceContext, LogEventLevel logEventLevel) =>
		Logs(testCorrelatorContext, logEventLevel, logEvent => GetSourceContext(logEvent) == sourceContext);

	private static IEnumerable<LogEntry> Logs(ITestCorrelatorContext testCorrelatorContext, LogEventLevel logEventLevel, Func<LogEvent, bool> expression) =>
		TestCorrelator
			.GetLogEventsFromContextGuid(testCorrelatorContext.Guid)
			.Where(expression)
			.Where(logEvent => logEvent.Level == logEventLevel)
			.Select(logEvent =>
			{
				var message = RenderWithoutQuotes(logEvent);
				return new LogEntry(message, logEventLevel, logEvent.Exception?.GetType());
			});

	private static string GetSourceContext(LogEvent logEvent)
	{
		var sourceContext = (ScalarValue)logEvent.Properties["SourceContext"];
		return sourceContext.Value.ToString();
	}

	private static string RenderWithoutQuotes(LogEvent logEvent)
	{
		using var output = new StringWriter();

		var formatter = new MessageTemplateTextFormatter(logEvent.MessageTemplate.Text);
		formatter.Format(logEvent, output);

		return output.ToString();
	}
}
