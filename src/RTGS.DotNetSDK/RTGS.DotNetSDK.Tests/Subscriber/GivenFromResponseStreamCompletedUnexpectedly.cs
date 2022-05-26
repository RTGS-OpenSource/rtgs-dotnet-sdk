using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.DotNetSDK.Subscriber;
using RTGS.DotNetSDK.Subscriber.Exceptions;
using RTGS.DotNetSDK.Subscriber.HandleMessageCommands;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.Validators;
using RTGS.DotNetSDK.Tests.Helper;
using Xunit;

namespace RTGS.DotNetSDK.Tests.Subscriber;

public class GivenFromResponseStreamCompletedUnexpectedly : IAsyncLifetime
{
	private readonly FakeLogger<RtgsSubscriber> _fakeLogger = new();
	private ExceptionEventArgs? _raisedArgs;
	private readonly ManualResetEventSlim _raisedExceptionSignal = new();
	private readonly TimeSpan _waitForExceptionDuration = TimeSpan.FromSeconds(30);

	public async Task InitializeAsync()
	{
		var paymentClient = new MockPaymentClient();
		var options = RtgsSdkOptions.Builder.CreateNew("did", new Uri("http://localhost"), new Uri("http://localhost")).Build();

		var rtgsSubscriber = new RtgsSubscriber(_fakeLogger, paymentClient.Object, options,
			Mock.Of<IHandlerValidator>(), Mock.Of<IHandleMessageCommandsFactory>());

		paymentClient.MockFromResponseStream.Setup(s => s.MoveNext(It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		rtgsSubscriber.OnExceptionOccurred += OnExceptionOccurredHandler;

		await rtgsSubscriber.StartAsync(Enumerable.Empty<IHandler>());
		_raisedExceptionSignal.Wait(_waitForExceptionDuration);
	}

	public Task DisposeAsync()
	{
		_raisedExceptionSignal.Dispose();
		return Task.CompletedTask;
	}

	private void OnExceptionOccurredHandler(object? _, ExceptionEventArgs? args)
	{
		_raisedArgs = args;
		_raisedExceptionSignal.Set();
	}

	[Fact]
	public void ThenThrowRtgsSubscriberException()
	{
		_raisedArgs.Should().NotBeNull();

		using var _ = new AssertionScope();

		_raisedArgs?.Exception.Should().BeOfType<RtgsSubscriberException>();
		_raisedArgs?.IsFatal.Should().BeTrue();
	}

	[Fact]
	public void ThenLogError() => _fakeLogger.Logs[LogLevel.Error].Should()
		.BeEquivalentTo("The subscriber was not stopped but the call was unexpectedly completed");
}
