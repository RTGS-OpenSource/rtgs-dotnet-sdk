using Grpc.Core;
using Moq;
using RTGS.Public.Payment.V4;

namespace RTGS.DotNetSDK.Tests.Helper;

public class MockPaymentClient : Mock<Payment.PaymentClient>
{
	public Mock<IAsyncStreamReader<RtgsMessage>> MockFromResponseStream { get; } = new();

	public MockPaymentClient()
	{
		var fromStream = new AsyncDuplexStreamingCall<RtgsMessageAcknowledgement, RtgsMessage>(
			default,
			MockFromResponseStream.Object,
			Task.FromResult(Metadata.Empty),
			default,
			default,
			() => { }
		);

		Setup(c => c.FromRtgsMessage(It.IsAny<Metadata>(), default, It.IsAny<CancellationToken>()))
			.Returns(fromStream);
	}
}
