using Microsoft.Extensions.Logging;
using System;
using Xunit.Abstractions;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.Logging
{
	internal class XUnitLogger : ILogger
	{
		private readonly ITestOutputHelper _outputHelper;
		private readonly string _categoryName;

		public XUnitLogger(ITestOutputHelper outputHelper, string categoryName)
		{
			_outputHelper = outputHelper;
			_categoryName = categoryName;
		}

		public IDisposable BeginScope<TState>(TState state) => null;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			_outputHelper.WriteLine($"{_categoryName} [{eventId}] {formatter(state, exception)}");

			if (exception != null)
			{
				_outputHelper.WriteLine(exception.ToString());
			}
		}
	}
}
