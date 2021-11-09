
namespace RTGSDotNetSDK.Publisher.IntegrationTests
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