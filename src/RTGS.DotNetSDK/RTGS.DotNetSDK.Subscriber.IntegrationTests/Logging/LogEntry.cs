using System;
using Serilog.Events;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.Logging
{
	public record LogEntry(string Message, LogEventLevel LogLevel, Type ExceptionType = null);
}
