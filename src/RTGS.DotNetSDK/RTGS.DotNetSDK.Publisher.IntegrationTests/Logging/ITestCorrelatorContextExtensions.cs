using Serilog.Formatting.Display;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.Logging;

public static class ITestCorrelatorContextExtensions
{
	public static IEnumerable<LogEntry> PublisherLogs(this ITestCorrelatorContext testCorrelatorContext, LogEventLevel logEventLevel) =>
		Logs(testCorrelatorContext, "RTGS.DotNetSDK.Publisher.MessagePublisher", logEventLevel);

	public static IEnumerable<LogEntry> ConnectionBrokerLogs(this ITestCorrelatorContext testCorrelatorContext, LogEventLevel logEventLevel) =>
		Logs(testCorrelatorContext, "RTGS.DotNetSDK.Publisher.RtgsConnectionBroker", logEventLevel);

	private static IEnumerable<LogEntry> Logs(ITestCorrelatorContext testCorrelatorContext, string sourceContext, LogEventLevel logEventLevel) =>
		TestCorrelator
			.GetLogEventsFromContextGuid(testCorrelatorContext.Guid)
			.Where(logEvent => GetSourceContext(logEvent) == sourceContext)
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
