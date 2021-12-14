using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.DotNetSDK.Subscriber.Extensions;
using RTGS.DotNetSDK.Subscriber.HandleMessageCommands;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.Logging;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using Xunit;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests
{
	public class GivenUnexpectedException : IAsyncDisposable
	{
		private readonly OutOfMemoryException _thrownException;
		private readonly ITestCorrelatorContext _serilogContext;
		private readonly IHost _clientHost;
		private readonly IRtgsSubscriber _rtgsSubscriber;

		public GivenUnexpectedException()
		{
			SetupSerilogLogger();

			_serilogContext = TestCorrelator.CreateContext();

			var rtgsSubscriberOptions = RtgsSubscriberOptions.Builder.CreateNew(ValidMessages.BankDid, new Uri("https://localhost:4567"))
				.Build();

			_thrownException = new OutOfMemoryException("test");

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices(services =>
				{
					services.AddRtgsSubscriber(rtgsSubscriberOptions);

					services.AddTransient<IHandleMessageCommandsFactory>(_ => new ThrowHandleMessageCommandsFactory(_thrownException));
				})
				.UseSerilog()
				.Build();

			_rtgsSubscriber = _clientHost.Services.GetRequiredService<IRtgsSubscriber>();
		}

		private class ThrowHandleMessageCommandsFactory : IHandleMessageCommandsFactory
		{
			private readonly Exception _exception;

			public ThrowHandleMessageCommandsFactory(Exception exception)
			{
				_exception = exception;
			}

			public IEnumerable<IHandleMessageCommand> CreateAll(IReadOnlyCollection<IHandler> handlers) =>
				throw _exception;
		}

		private static void SetupSerilogLogger() =>
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.WriteTo.TestCorrelator()
				.CreateLogger();

		public async ValueTask DisposeAsync()
		{
			await _rtgsSubscriber.DisposeAsync();
			await _clientHost.StopAsync();
		}

		[Fact]
		public async Task WhenStarting_ThenExceptionEventIsRaised()
		{
			using var raisedExceptionSignal = new ManualResetEventSlim();
			Exception raisedException = null;

			_rtgsSubscriber.OnExceptionOccurred += (_, args) =>
			{
				raisedException = args.Exception;
				raisedExceptionSignal.Set();
			};

			await _rtgsSubscriber.StartAsync(new AllTestHandlers());

			var waitForExceptionDuration = TimeSpan.FromSeconds(30);
			raisedExceptionSignal.Wait(waitForExceptionDuration);

			raisedException.Should().NotBeNull().And.Be(_thrownException);
		}

		[Fact]
		public async Task WhenStarting_ThenExceptionIsLogged()
		{
			using var raisedExceptionSignal = new ManualResetEventSlim();

			_rtgsSubscriber.OnExceptionOccurred += (_, _) => raisedExceptionSignal.Set();

			await _rtgsSubscriber.StartAsync(new AllTestHandlers());

			var waitForExceptionDuration = TimeSpan.FromSeconds(30);
			raisedExceptionSignal.Wait(waitForExceptionDuration);

			var errorLogs = _serilogContext.SubscriberLogs(LogEventLevel.Error);
			errorLogs.Should().BeEquivalentTo(new[] { new LogEntry("An unknown error occurred", LogEventLevel.Error, _thrownException.GetType()) });
		}
	}
}
