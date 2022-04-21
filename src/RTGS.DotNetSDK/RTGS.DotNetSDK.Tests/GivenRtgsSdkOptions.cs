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
					new Uri("http://id-crypt-cloud-agent.com"),
					"idcrypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.KeepAlivePingDelay(duration))
			.Should()
			.Throw<ArgumentOutOfRangeException>()
			.WithMessage($"Value must be at least 1 second. (Parameter 'duration'){Environment.NewLine}Actual value was {duration.TotalSeconds}.");

	[Fact]
	public void WhenKeepAlivePingDelayIsInfinite_ThenDoNotThrowArgumentOutOfRangeException() =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("http://id-crypt-cloud-agent.com"),
					"idcrypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.KeepAlivePingDelay(Timeout.InfiniteTimeSpan))
			.Should()
			.NotThrow<ArgumentOutOfRangeException>();

	[Theory]
	[MemberData(nameof(TimeSpansAtLeastOneSecond))]
	public void WhenKeepAlivePingDelayIsAtLeastOneSecond_ThenDoNotThrowArgumentOutOfRangeException(TimeSpan duration) =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("http://id-crypt-cloud-agent.com"),
					"idcrypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.KeepAlivePingDelay(duration))
			.Should()
			.NotThrow<ArgumentOutOfRangeException>();

	[Theory]
	[MemberData(nameof(TimeSpansLessThanOneSecond))]
	public void WhenKeepAlivePingTimeoutLessThanOneSecond_ThenThrowArgumentOutOfRangeException(TimeSpan duration) =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("http://id-crypt-cloud-agent.com"),
					"idcrypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.KeepAlivePingTimeout(duration))
			.Should()
			.Throw<ArgumentOutOfRangeException>()
			.WithMessage($"Value must be at least 1 second. (Parameter 'duration'){Environment.NewLine}Actual value was {duration.TotalSeconds}.");

	[Fact]
	public void WhenKeepAlivePingTimeoutIsInfinite_ThenDoNotThrowArgumentOutOfRangeException() =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("http://id-crypt-cloud-agent.com"),
					"idcrypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.KeepAlivePingTimeout(Timeout.InfiniteTimeSpan))
			.Should()
			.NotThrow<ArgumentOutOfRangeException>();

	[Theory]
	[MemberData(nameof(TimeSpansAtLeastOneSecond))]
	public void WhenKeepAlivePingTimeoutIsAtLeastOneSecond_ThenDoNotThrowArgumentOutOfRangeException(TimeSpan duration) =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("http://id-crypt-cloud-agent.com"),
					"idcrypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.KeepAlivePingTimeout(duration))
			.Should()
			.NotThrow<ArgumentOutOfRangeException>();

	[Fact]
	public void WhenRemoteHostAddressIsNull_ThenThrowArgumentNullException() =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
				"rtgs-global-did",
				null,
				new Uri("http://id-crypt-cloud-agent.com"),
				"idcrypt-api-key",
				new Uri("http://id-crypt-cloud-agent-service-endpoint.com")))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null. (Parameter 'remoteHostAddress')");

	[Fact]
	public void WhenRtgsGlobalIdIsNull_ThenThrowArgumentNullException() =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
				null,
				new Uri("http://example.org"),
				new Uri("http://id-crypt-cloud-agent.com"),
				"idcrypt-api-key",
				new Uri("http://id-crypt-cloud-agent-service-endpoint.com")))
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
				new Uri("http://id-crypt-cloud-agent.com"),
				"idcrypt-api-key",
				new Uri("http://id-crypt-cloud-agent-service-endpoint.com")))
			.Should()
			.Throw<ArgumentException>()
			.WithMessage("Value cannot be white space. (Parameter 'rtgsGlobalId')");

	[Theory]
	[MemberData(nameof(TimeSpansLessThanOneSecond))]
	public void WhenWaitForAcknowledgementDurationIsLessThanOneSecond_ThenThrowArgumentOutOfRangeException(TimeSpan duration) =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("http://id-crypt-cloud-agent.com"),
					"idcrypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.WaitForAcknowledgementDuration(duration))
			.Should()
			.Throw<ArgumentOutOfRangeException>()
			.WithMessage($"Value must be between 1 and 30 seconds. (Parameter 'duration'){Environment.NewLine}Actual value was {duration.TotalSeconds}.");

	[Fact]
	public void WhenWaitForAcknowledgementDurationIsInfinite_ThenThrowArgumentOutOfRangeException() =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
					"rtgs-global-did",
					new Uri("http://example.org"),
					new Uri("http://id-crypt-cloud-agent.com"),
					"idcrypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
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
					new Uri("http://id-crypt-cloud-agent.com"),
					"idcrypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.WaitForAcknowledgementDuration(duration))
			.Should()
			.Throw<ArgumentOutOfRangeException>()
			.WithMessage($"Value must be between 1 and 30 seconds. (Parameter 'duration'){Environment.NewLine}Actual value was {duration.TotalSeconds}.");

	[Fact]
	public void WhenIdCryptApiAddressIsNull_ThenThrowArgumentNullException() =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
				"rtgs-global-did",
				new Uri("http://example.org"),
				null,
				"idcrypt-api-key",
				new Uri("http://id-crypt-cloud-agent-service-endpoint.com")))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null. (Parameter 'idCryptApiAddress')");

	[Fact]
	public void WhenIdCryptApiKeyIsNull_ThenThrowArgumentNullException() =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
				"rtgs-global-did",
				new Uri("http://example.org"),
				new Uri("http://id-crypt-cloud-agent.com"),
				null,
				new Uri("http://id-crypt-cloud-agent-service-endpoint.com")))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null. (Parameter 'idCryptApiKey')");

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	public void WhenIdCryptApiKeyIsWhiteSpace_ThenThrowArgumentException(string whiteSpace) =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
				"rtgs-global-did",
				new Uri("http://example.org"),
				new Uri("http://id-crypt-cloud-agent.com"),
				whiteSpace,
				new Uri("http://id-crypt-cloud-agent-service-endpoint.com")))
			.Should()
			.Throw<ArgumentException>()
			.WithMessage("Value cannot be white space. (Parameter 'idCryptApiKey')");

	[Fact]
	public void WhenIdServiceEndpointAddressIsNull_ThenThrowArgumentNullException() =>
		FluentActions.Invoking(() => RtgsSdkOptions.Builder.CreateNew(
				"rtgs-global-did",
				new Uri("http://example.org"),
				new Uri("http://id-crypt-cloud-agent.com"),
				"idcrypt-api-key",
				null))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null. (Parameter 'idCryptServiceEndpointAddress')");

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
