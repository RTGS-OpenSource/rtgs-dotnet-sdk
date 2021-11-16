using FluentAssertions;
using FluentAssertions.Execution;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.Public.Payment.V2;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RTGS.DotNetSDK.Publisher.Tests
{
	public class GivenRtgsPublisher
	{
		private readonly FakeLogger<RtgsPublisher> _fakeLogger = new();
		private readonly RtgsClientOptions _rtgsClientOptions;

		public GivenRtgsPublisher()
		{
			_rtgsClientOptions = RtgsClientOptions.Builder.CreateNew()
				.BankDid("")
				.RemoteHost("http://test.me")
				.Build();
		}

		[Fact]
		public async Task WhenToRtgsCallInitialisationFails_ThenLogErrorAndReturnResult()
		{
			var mockClient = new Mock<Payment.PaymentClient>();

			mockClient.Setup(handler =>
					handler.ToRtgsMessage(It.IsAny<Metadata>(), default, It.IsAny<CancellationToken>()))
				.Throws(new Exception());

			var rtgsPublisher = new RtgsPublisher(_fakeLogger, mockClient.Object, _rtgsClientOptions);

			var result = await rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequest(), default);

			using var _ = new AssertionScope();

			_fakeLogger.Logs[LogLevel.Error].Should().BeEquivalentTo(new[]
			{
				"Failed to connect to RTGS"
			});

			result.Should().Be(SendResult.ConnectionError);
		}

		[Fact]
		public async Task WhenWriteToRequestStreamFails_ThenLogErrorAndReturnResult()
		{
			var mockClient = new Mock<Payment.PaymentClient>();
			var mockToRequestStream = new Mock<IClientStreamWriter<RtgsMessage>>();

			mockToRequestStream
				.Setup(stream => stream.WriteAsync(It.IsAny<RtgsMessage>()))
				.Throws(new Exception());

			var mockToStream = new AsyncDuplexStreamingCall<RtgsMessage, RtgsMessageAcknowledgement>(
				mockToRequestStream.Object,
				null,
				Task.FromResult(Metadata.Empty),
				default,
				default,
				() => { });

			mockClient.Setup(handler =>
					handler.ToRtgsMessage(It.IsAny<Metadata>(), default, It.IsAny<CancellationToken>()))
				.Returns(mockToStream);

			var rtgsPublisher = new RtgsPublisher(_fakeLogger, mockClient.Object, _rtgsClientOptions);

			var result = await rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequest(), default);

			using var _ = new AssertionScope();

			_fakeLogger.Logs[LogLevel.Error].Should().BeEquivalentTo(new[]
			{
				$"Error sending {typeof(AtomicLockRequest).Name} to RTGS"
			});

			result.Should().Be(SendResult.ClientError);
		}

		[Fact]
		public async Task WhenSendRequestAsyncSucceeds_ThenLogInformation()
		{
			var mockClient = new Mock<Payment.PaymentClient>();
			var mockToRequestStream = new Mock<IClientStreamWriter<RtgsMessage>>();

			var mockToStream = new AsyncDuplexStreamingCall<RtgsMessage, RtgsMessageAcknowledgement>(
				mockToRequestStream.Object,
				null,
				Task.FromResult(Metadata.Empty),
				default,
				default,
				() => { });

			mockClient.Setup(handler =>
					handler.ToRtgsMessage(It.IsAny<Metadata>(), default, It.IsAny<CancellationToken>()))
				.Returns(mockToStream);

			var rtgsPublisher = new RtgsPublisher(_fakeLogger, mockClient.Object, _rtgsClientOptions);

			var result = await rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequest(), default);

			_fakeLogger.Logs[LogLevel.Information].Should().BeEquivalentTo(new[]
			{
				$"Connecting to RTGS",
				$"Connected to RTGS",
				$"Sending {typeof(AtomicLockRequest).Name} to RTGS",
				$"Sent {typeof(AtomicLockRequest).Name} to RTGS",
			});
		}
	}
}
