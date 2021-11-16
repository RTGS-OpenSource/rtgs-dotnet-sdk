using Xunit;

namespace RTGS.DotNetSDK.Publisher.Tests
{
	public class GivenRtgsPublisher
    {
		private readonly FakeLogger<RtgsPublisher> _fakeLogger = new();
		private readonly RtgsPublisher _rtgsPublisher;

		public GivenRtgsPublisher()
		{
			var rtgsClientOptions = RtgsClientOptions.Builder.CreateNew()
				.BankDid("")
				.RemoteHost("")
				.Build();

			_rtgsPublisher = new RtgsPublisher(_fakeLogger, null, rtgsClientOptions);
		}

		[Fact]
		public void WhenToRtgsCallInitialisationFails_ThenLogFailure()
		{

		}
    }
}
