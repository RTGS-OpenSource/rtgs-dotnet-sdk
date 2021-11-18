﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting.Display;
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
				.Select(RenderWithoutQuotes)
				.Select(message => new LogEntry(message));

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
}