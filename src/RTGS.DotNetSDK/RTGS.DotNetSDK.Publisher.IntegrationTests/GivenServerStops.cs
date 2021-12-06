using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.DotNetSDK.Publisher.Extensions;
using RTGS.DotNetSDK.Publisher.IntegrationTests.Logging;
using RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;
using RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer;
using RTGS.DotNetSDK.Publisher.Messages;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using Xunit;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class GivenServerStops : IAsyncLifetime
	{
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

		private readonly GrpcTestServer _grpcServer;
		private readonly ITestCorrelatorContext _serilogContext;
		private IRtgsPublisher _rtgsPublisher;
		private ToRtgsMessageHandler _toRtgsMessageHandler;
		private IHost _clientHost;

		public GivenServerStops()
		{
			_grpcServer = new GrpcTestServer();

			SetupSerilogLogger();

			_serilogContext = TestCorrelator.CreateContext();
		}

		private static void SetupSerilogLogger() =>
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.WriteTo.TestCorrelator()
				.CreateLogger();

		public async Task InitializeAsync()
		{
			try
			{
				var serverUri = await _grpcServer.StartAsync();

				var rtgsClientOptions = RtgsPublisherOptions.Builder.CreateNew(ValidRequests.BankDid, serverUri)
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices((_, services) => services.AddRtgsPublisher(rtgsClientOptions))
					.UseSerilog()
					.Build();

				_rtgsPublisher = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
				_toRtgsMessageHandler = _grpcServer.Services.GetRequiredService<ToRtgsMessageHandler>();
			}
			catch (Exception)
			{
				// If an exception occurs then manually clean up as IAsyncLifetime.DisposeAsync is not called.
				// See https://github.com/xunit/xunit/discussions/2313 for further details.
				await DisposeAsync();

				throw;
			}
		}

		public async Task DisposeAsync()
		{
			if (_rtgsPublisher is not null)
			{
				await _rtgsPublisher.DisposeAsync();
			}

			_clientHost?.Dispose();
			_grpcServer.Dispose();
		}

		[Fact]
		public async Task WhenSendingMessage_ThenRpcExceptionOrIOExceptionThrown()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			var result = await _rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequest());
			result.Should().Be(SendResult.Success);

			await _grpcServer.StopAsync();

			var exceptionAssertions = await FluentActions.Awaiting(() => _rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequest()))
				.Should().ThrowAsync<Exception>();

			// One of two exceptions can be thrown depending on how far along the call is.
			exceptionAssertions.And.GetType()
				.Should().Match(exceptionType => exceptionType == typeof(RpcException)
												 || exceptionType == typeof(IOException));
		}

		[Fact]
		public async Task WhenNotSendingMessage_ThenLogError()
		{
			_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

			await _rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequest());

			await _grpcServer.StopAsync();

			await _rtgsPublisher.DisposeAsync();

			var errorLogs = _serilogContext.PublisherLogs(LogEventLevel.Error);
			errorLogs.Should().BeEquivalentTo(new[]
			{
				new LogEntry("RTGS connection unexpectedly closed", LogEventLevel.Error, typeof(RpcException))
			});
		}
	}
}
