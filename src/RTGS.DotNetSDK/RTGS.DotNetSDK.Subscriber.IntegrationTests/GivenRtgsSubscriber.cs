using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.DotNetSDK.Subscriber.Extensions;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestServer;
using Xunit;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests
{
	public class GivenRtgsSubscriber : IAsyncLifetime, IClassFixture<GrpcServerFixture>
	{
		private readonly GrpcServerFixture _grpcServer;
		private IHost _clientHost;
		private IRtgsSubscriber _rtgsSubscriber;

		public GivenRtgsSubscriber(GrpcServerFixture grpcServer)
		{
			_grpcServer = grpcServer;
		}

		public async Task InitializeAsync()
		{
			try
			{
				var rtgsSubscriberOptions = RtgsSubscriberOptions.Builder.CreateNew(ValidMessages.BankDid, _grpcServer.ServerUri)
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
					.ConfigureServices((_, services) => services.AddRtgsSubscriber(rtgsSubscriberOptions))
					.Build();

				_rtgsSubscriber = _clientHost.Services.GetRequiredService<IRtgsSubscriber>();
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
			await _rtgsSubscriber.DisposeAsync();

			_clientHost?.Dispose();

			_grpcServer.Reset();
		}

		[Fact]
		public void WhenStartIsCalledTwice_ThenThrowInvalidOperationException()
		{
			_rtgsSubscriber.Start(new AllTestHandlers());

			FluentActions.Invoking(() => _rtgsSubscriber.Start(new AllTestHandlers()))
				.Should().ThrowExactly<InvalidOperationException>()
				.WithMessage("RTGS Subscriber is already running");
		}

		[Fact]
		public async Task WhenStopIsCalledButSubscriberNotRunning_ThenThrowInvalidOperationExcpetion() =>
			await FluentActions.Awaiting(() => _rtgsSubscriber.StopAsync())
				.Should().ThrowExactlyAsync<InvalidOperationException>()
				.WithMessage("RTGS Subscriber is not running");

		[Fact]
		public async Task AndSubscriberHasBeenDisposed_WhenStarting_ThenThrow()
		{
			_rtgsSubscriber.Start(new AllTestHandlers());

			await _rtgsSubscriber.DisposeAsync();

			FluentActions.Invoking(() => _rtgsSubscriber.Start(new AllTestHandlers()))
				.Should().ThrowExactly<ObjectDisposedException>()
				.WithMessage("*RtgsSubscriber*");
		}

		[Fact]
		public async Task AndSubscriberHasBeenDisposed_WhenStopping_ThenThrow()
		{
			_rtgsSubscriber.Start(new AllTestHandlers());

			await _rtgsSubscriber.DisposeAsync();

			await FluentActions.Awaiting(() => _rtgsSubscriber.StopAsync())
				.Should().ThrowExactlyAsync<ObjectDisposedException>()
				.WithMessage("*RtgsSubscriber*");
		}
	}
}
