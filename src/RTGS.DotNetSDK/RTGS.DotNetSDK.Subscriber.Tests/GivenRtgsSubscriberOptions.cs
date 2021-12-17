﻿using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace RTGS.DotNetSDK.Subscriber.Tests;

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

	[Theory]
	[MemberData(nameof(TimeSpansLessThanOneSecond))]
	public void WhenKeepAlivePingDelayIsLessThanOneSecond_ThenThrowArgumentOutOfRangeException(TimeSpan duration) =>
		FluentActions.Invoking(() => RtgsSubscriberOptions.Builder.CreateNew("bank-did", new Uri("http://example.org"))
				.KeepAlivePingDelay(duration))
			.Should()
			.Throw<ArgumentOutOfRangeException>()
			.WithMessage($"Value must be at least 1 second. (Parameter 'duration'){Environment.NewLine}Actual value was {duration.TotalSeconds}.");

	[Fact]
	public void WhenKeepAlivePingDelayIsInfinite_ThenDoNotThrowArgumentOutOfRangeException() =>
		FluentActions.Invoking(() => RtgsSubscriberOptions.Builder.CreateNew("bank-did", new Uri("http://example.org"))
				.KeepAlivePingDelay(Timeout.InfiniteTimeSpan))
			.Should()
			.NotThrow<ArgumentOutOfRangeException>();

	[Theory]
	[MemberData(nameof(TimeSpansAtLeastOneSecond))]
	public void WhenKeepAlivePingDelayIsAtLeastOneSecond_ThenDoNotThrowArgumentOutOfRangeException(TimeSpan duration) =>
		FluentActions.Invoking(() => RtgsSubscriberOptions.Builder.CreateNew("bank-did", new Uri("http://example.org"))
				.KeepAlivePingDelay(duration))
			.Should()
			.NotThrow<ArgumentOutOfRangeException>();

	[Theory]
	[MemberData(nameof(TimeSpansLessThanOneSecond))]
	public void WhenKeepAlivePingTimeoutLessThanOneSecond_ThenThrowArgumentOutOfRangeException(TimeSpan duration) =>
		FluentActions.Invoking(() => RtgsSubscriberOptions.Builder.CreateNew("bank-did", new Uri("http://example.org"))
				.KeepAlivePingTimeout(duration))
			.Should()
			.Throw<ArgumentOutOfRangeException>()
			.WithMessage($"Value must be at least 1 second. (Parameter 'duration'){Environment.NewLine}Actual value was {duration.TotalSeconds}.");

	[Fact]
	public void WhenKeepAlivePingTimeoutIsInfinite_ThenDoNotThrowArgumentOutOfRangeException() =>
		FluentActions.Invoking(() => RtgsSubscriberOptions.Builder.CreateNew("bank-did", new Uri("http://example.org"))
				.KeepAlivePingTimeout(Timeout.InfiniteTimeSpan))
			.Should()
			.NotThrow<ArgumentOutOfRangeException>();

	[Theory]
	[MemberData(nameof(TimeSpansAtLeastOneSecond))]
	public void WhenKeepAlivePingTimeoutIsAtLeastOneSecond_ThenDoNotThrowArgumentOutOfRangeException(TimeSpan duration) =>
		FluentActions.Invoking(() => RtgsSubscriberOptions.Builder.CreateNew("bank-did", new Uri("http://example.org"))
				.KeepAlivePingTimeout(duration))
			.Should()
			.NotThrow<ArgumentOutOfRangeException>();

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
}
