using Serilog.Events;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.Logging
{
	public record LogEntry(string Message, LogEventLevel LogLevel);
}
