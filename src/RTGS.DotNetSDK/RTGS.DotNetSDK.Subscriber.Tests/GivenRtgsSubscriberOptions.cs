using System;
using FluentAssertions;
using Xunit;

namespace RTGS.DotNetSDK.Subscriber.Tests
{
	public class GivenRtgsSubscriberOptions
	{
		[Fact]
		public void WhenRemoteHostAddressIsNull_ThenThrowArgumentNullException() =>
			FluentActions.Invoking(() => RtgsSubscriberOptions.Builder.CreateNew("bank-did", null))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null. (Parameter 'remoteHostAddress')");

		[Fact]
		public void WhenBankDidIsNull_ThenThrowArgumentNullException() =>
			FluentActions.Invoking(() => RtgsSubscriberOptions.Builder.CreateNew(null, new Uri("http://example.org")))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null. (Parameter 'bankDid')");

		[Theory]
		[InlineData("")]
		[InlineData(" ")]
		public void WhenBankDidIsWhiteSpace_ThenThrowArgumentException(string whiteSpace) =>
			FluentActions.Invoking(() => RtgsSubscriberOptions.Builder.CreateNew(whiteSpace, new Uri("http://example.org")))
				.Should()
				.Throw<ArgumentException>()
				.WithMessage("Value cannot be white space. (Parameter 'bankDid')");
	}
}
