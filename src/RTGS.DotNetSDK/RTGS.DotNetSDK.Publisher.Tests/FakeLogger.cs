using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RTGS.DotNetSDK.Publisher.Tests
{
	public class FakeLogger<T> : ILogger<T>
	{
		public FakeLogger()
		{
			Logs = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().ToDictionary(level => level, _ => new List<string>());
		}
		public IDictionary<LogLevel, List<string>> Logs { get; }
		public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();
		public bool IsEnabled(LogLevel logLevel) => true;
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
			Logs[logLevel].Add(state.ToString());
	}
}
