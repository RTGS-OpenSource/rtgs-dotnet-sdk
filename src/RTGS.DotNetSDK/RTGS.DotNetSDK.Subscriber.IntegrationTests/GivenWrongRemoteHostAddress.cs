using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.DotNetSDK.Subscriber.Extensions;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.Logging;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using Xunit;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests
{
	public class GivenWrongRemoteHostAddress : IAsyncDisposable
	{
		private readonly ITestCorrelatorContext _serilogContext;
		private readonly IHost _clientHost;
		private readonly IRtgsSubscriber _rtgsSubscriber;

		public GivenWrongRemoteHostAddress()
		{
			SetupSerilogLogger();

			_serilogContext = TestCorrelator.CreateContext();

			var rtgsSubscriberOptions = RtgsSubscriberOptions.Builder.CreateNew(ValidMessages.BankDid, new Uri("https://localhost:4567"))
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices(services => services.AddRtgsSubscriber(rtgsSubscriberOptions))
				.UseSerilog()
				.Build();

			_rtgsSubscriber = _clientHost.Services.GetRequiredService<IRtgsSubscriber>();
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

			raisedException.Should().NotBeNull();
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
			errorLogs.Should().BeEquivalentTo(new[] { new LogEntry("An error occurred while communicating with RTGS", LogEventLevel.Error, typeof(RpcException)) });
		}
	}
}
