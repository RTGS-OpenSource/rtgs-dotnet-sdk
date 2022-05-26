using Microsoft.Extensions.Logging;

namespace RTGS.DotNetSDK.Tests.Helper;

public sealed class FakeLogger<T> : ILogger<T>, IDisposable
{
	private bool _isDisposed;
	private readonly IDictionary<LogLevel, List<string>> _logs;
	private readonly IDictionary<LogLevel, ManualResetEventSlim> _logEvents;

	public FakeLogger()
	{
		_logs = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().ToDictionary(level => level, _ => new List<string>());
		_logEvents = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().ToDictionary(level => level, _ => new ManualResetEventSlim());
	}

	public IDictionary<LogLevel, List<string>> Logs
	{
		get
		{
			ThrowIfDisposed();

			return _logs;
		}
	}

	public IDictionary<LogLevel, ManualResetEventSlim> LogEvents
	{
		get
		{
			ThrowIfDisposed();

			return _logEvents;
		}
	}

	public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();

	public bool IsEnabled(LogLevel logLevel)
	{
		ThrowIfDisposed();

		return true;
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
	{
		ThrowIfDisposed();

		Logs[logLevel].Add(state.ToString());
		LogEvents[logLevel].Set();
	}

	private void ThrowIfDisposed()
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException("Logger has been disposed");
		}
	}

	public void Dispose()
	{
		if (_isDisposed)
		{
			return;
		}

		foreach ((LogLevel _, ManualResetEventSlim value) in LogEvents)
		{
			value.Dispose();
		}

		_isDisposed = true;
	}
}
