using System.Collections.Generic;
using System.Linq;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.Logging
{
	public static class ITestCorrelatorContextExtensions
	{
		public static IEnumerable<LogEntry> PublisherLogs(this ITestCorrelatorContext testCorrelatorContext, LogEventLevel logEventLevel) =>
			TestCorrelator
				.GetLogEventsFromContextGuid(testCorrelatorContext.Guid)
				.Where(logEvent => GetSourceContext(logEvent) == "RTGS.DotNetSDK.Publisher.RtgsPublisher")
				.Where(logEvent => logEvent.Level == logEventLevel)
				.Select(logEvent => new LogEntry(logEvent.RenderMessage()));

		private static string GetSourceContext(LogEvent logEvent)
		{
			var sourceContext = (ScalarValue)logEvent.Properties["SourceContext"];
			return sourceContext.Value.ToString();
		}
	}	
}
