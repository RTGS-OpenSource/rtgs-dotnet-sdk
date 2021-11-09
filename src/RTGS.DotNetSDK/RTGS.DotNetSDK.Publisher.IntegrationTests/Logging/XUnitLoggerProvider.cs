using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.Logging
{
	internal class XUnitLoggerProvider : ILoggerProvider
	{
		private readonly ITestOutputHelper _outputHelper;

		public XUnitLoggerProvider(ITestOutputHelper outputHelper)
		{
			_outputHelper = outputHelper;
		}

		public ILogger CreateLogger(string categoryName) => new XUnitLogger(_outputHelper, categoryName);

		public void Dispose() => Expression.Empty();
	}
}
