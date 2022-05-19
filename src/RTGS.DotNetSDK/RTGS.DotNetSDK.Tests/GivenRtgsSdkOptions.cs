using FluentAssertions;
using Xunit;

namespace RTGS.DotNetSDK.Tests;

public class GivenRtgsSdkOptions
{
	[Theory]
	[MemberData(nameof(TimeSpansLessThanOneSecond))]
	public void WhenKeepAlivePingDelayIsLessThanOneSecond_ThenThrowArgumentOutOfRangeException(TimeSpan duration) =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("https://id-crypt-service"))
				.KeepAlivePingDelay(duration))
			.Should()
			.Throw<ArgumentOutOfRangeException>()
			.WithMessage($"Value must be at least 1 second. (Parameter 'duration'){Environment.NewLine}Actual value was {duration.TotalSeconds}.");

	[Fact]
	public void WhenKeepAlivePingDelayIsInfinite_ThenDoNotThrowArgumentOutOfRangeException() =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("https://id-crypt-service"))
				.KeepAlivePingDelay(Timeout.InfiniteTimeSpan))
			.Should()
			.NotThrow<ArgumentOutOfRangeException>();

	[Theory]
	[MemberData(nameof(TimeSpansAtLeastOneSecond))]
	public void WhenKeepAlivePingDelayIsAtLeastOneSecond_ThenDoNotThrowArgumentOutOfRangeException(TimeSpan duration) =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("https://id-crypt-service"))
				.KeepAlivePingDelay(duration))
			.Should()
			.NotThrow<ArgumentOutOfRangeException>();

	[Theory]
	[MemberData(nameof(TimeSpansLessThanOneSecond))]
	public void WhenKeepAlivePingTimeoutLessThanOneSecond_ThenThrowArgumentOutOfRangeException(TimeSpan duration) =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("https://id-crypt-service"))
				.KeepAlivePingTimeout(duration))
			.Should()
			.Throw<ArgumentOutOfRangeException>()
			.WithMessage($"Value must be at least 1 second. (Parameter 'duration'){Environment.NewLine}Actual value was {duration.TotalSeconds}.");

	[Fact]
	public void WhenKeepAlivePingTimeoutIsInfinite_ThenDoNotThrowArgumentOutOfRangeException() =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("https://id-crypt-service"))
				.KeepAlivePingTimeout(Timeout.InfiniteTimeSpan))
			.Should()
			.NotThrow<ArgumentOutOfRangeException>();

	[Theory]
	[MemberData(nameof(TimeSpansAtLeastOneSecond))]
	public void WhenKeepAlivePingTimeoutIsAtLeastOneSecond_ThenDoNotThrowArgumentOutOfRangeException(TimeSpan duration) =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("https://id-crypt-service"))
				.KeepAlivePingTimeout(duration))
			.Should()
			.NotThrow<ArgumentOutOfRangeException>();

	[Fact]
	public void WhenRemoteHostAddressIsNull_ThenThrowArgumentNullException() =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
				"rtgs-global-did",
				null,
				new Uri("https://id-crypt-service")))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null. (Parameter 'remoteHostAddress')");

	[Fact]
	public void WhenRtgsGlobalIdIsNull_ThenThrowArgumentNullException() =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
				null,
				new Uri("http://example.org"),
				new Uri("https://id-crypt-service")))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null. (Parameter 'rtgsGlobalId')");

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	public void WhenRtgsGlobalIdIsWhiteSpace_ThenThrowArgumentException(string whiteSpace) =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
				whiteSpace,
				new Uri("http://example.org"),
				new Uri("https://id-crypt-service")))
			.Should()
			.Throw<ArgumentException>()
			.WithMessage("Value cannot be white space. (Parameter 'rtgsGlobalId')");

	[Theory]
	[MemberData(nameof(TimeSpansLessThanOneSecond))]
	public void WhenWaitForAcknowledgementDurationIsLessThanOneSecond_ThenThrowArgumentOutOfRangeException(TimeSpan duration) =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("https://id-crypt-service"))
				.WaitForAcknowledgementDuration(duration))
			.Should()
			.Throw<ArgumentOutOfRangeException>()
			.WithMessage($"Value must be between 1 and 30 seconds. (Parameter 'duration'){Environment.NewLine}Actual value was {duration.TotalSeconds}.");

	[Fact]
	public void WhenWaitForAcknowledgementDurationIsInfinite_ThenThrowArgumentOutOfRangeException() =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("https://id-crypt-service"))
				.WaitForAcknowledgementDuration(Timeout.InfiniteTimeSpan))
			.Should()
			.Throw<ArgumentOutOfRangeException>()
			.WithMessage($"Value must be between 1 and 30 seconds. (Parameter 'duration'){Environment.NewLine}Actual value was -0.001.");

	[Theory]
	[MemberData(nameof(TimeSpansGreaterThanThirtySeconds))]
	public void WhenWaitForAcknowledgementDurationIsGreaterThan30_ThenThrowArgumentOutOfRangeException(TimeSpan duration) =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("https://id-crypt-service"))
				.WaitForAcknowledgementDuration(duration))
			.Should()
			.Throw<ArgumentOutOfRangeException>()
			.WithMessage($"Value must be between 1 and 30 seconds. (Parameter 'duration'){Environment.NewLine}Actual value was {duration.TotalSeconds}.");

	[Fact]
	public void WhenIdCryptServiceAddressIsNull_ThenThrowArgumentNullException() =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
				"rtgs-global-did",
				new Uri("http://example.org"),
				null))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null. (Parameter 'idCryptServiceAddress')");

	public static IEnumerable<object[]> TimeSpansLessThanOneSecond =>
		new List<object[]>
		{
			new object[] { TimeSpan.FromMilliseconds(0) },
			new object[] { TimeSpan.FromMilliseconds(999) },
			new object[] { TimeSpan.FromMilliseconds(100) }
		};

	public static IEnumerable<object[]> TimeSpansAtLeastOneSecond =>
		new List<object[]>
		{
			new object[] { TimeSpan.FromMilliseconds(1000) },
			new object[] { TimeSpan.FromMilliseconds(1001) },
			new object[] { TimeSpan.FromSeconds(10) }
		};

	public static IEnumerable<object[]> TimeSpansGreaterThanThirtySeconds =>
		new List<object[]>
		{
			new object[] { TimeSpan.FromMilliseconds(30001) },
			new object[] { TimeSpan.FromSeconds(40) }
		};
}
