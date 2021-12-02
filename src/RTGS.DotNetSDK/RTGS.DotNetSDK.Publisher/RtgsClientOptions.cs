using System;
using System.Threading;

namespace RTGS.DotNetSDK.Publisher
{
	/// <summary>
	/// Represents the options used when sending messages to RTGS via a <see cref="IRtgsPublisher"/>
	/// </summary>
	public record RtgsClientOptions
	{
		private RtgsClientOptions(Builder builder)
		{
			BankDid = builder.BankDidValue;
			RemoteHostAddress = builder.RemoteHostAddressValue;
			WaitForAcknowledgementDuration = builder.WaitForAcknowledgementDurationValue;
			KeepAlivePingDelay = builder.KeepAlivePingDelayValue;
			KeepAlivePingTimeout = builder.KeepAlivePingTimeoutValue;
		}

		/// <summary>
		/// Identifier of the bank.
		/// </summary>
		public string BankDid { get; }

		/// <summary>
		/// Grpc server endpoint
		/// </summary>
		public Uri RemoteHostAddress { get; }

		/// <summary>
		/// The gRPC acknowledgement timeout duration (default 10 seconds).
		/// If the time taken to send messages to RTGS exceeds this duration, the request will fail with an error.
		/// </summary>
		public TimeSpan WaitForAcknowledgementDuration { get; }

		/// <summary>
		/// The delay between each ping to keep the gRPC connection alive (default 30 seconds)
		/// </summary>
		public TimeSpan KeepAlivePingDelay { get; }

		/// <summary>
		/// The timeout period we expect a ping response within (default 30 seconds)
		/// </summary>
		public TimeSpan KeepAlivePingTimeout { get; }

		/// <summary>
		/// The Builder class
		/// </summary>
		public sealed class Builder
		{
			public Builder(string bankDid, Uri remoteHostAddress)
			{
				if (bankDid is null)
				{
					throw new ArgumentNullException(nameof(bankDid));
				}

				if (string.IsNullOrWhiteSpace(bankDid))
				{
					throw new ArgumentException("Value cannot be white space.", nameof(bankDid));
				}

				if (remoteHostAddress is null)
				{
					throw new ArgumentNullException(nameof(remoteHostAddress));
				}

				BankDidValue = bankDid;
				RemoteHostAddressValue = remoteHostAddress;
			}

			internal string BankDidValue { get; }
			internal Uri RemoteHostAddressValue { get; }
			internal TimeSpan WaitForAcknowledgementDurationValue { get; private set; } = TimeSpan.FromSeconds(10);
			internal TimeSpan KeepAlivePingDelayValue { get; private set; } = TimeSpan.FromSeconds(30);
			internal TimeSpan KeepAlivePingTimeoutValue { get; private set; } = TimeSpan.FromSeconds(30);

			/// <summary>
			/// Creates a new builder
			/// </summary>
			/// <param name="bankDid"></param>
			/// <param name="remoteHostAddress"></param>
			/// <returns>The builder</returns>
			public static Builder CreateNew(string bankDid, Uri remoteHostAddress) => new(bankDid, remoteHostAddress);

			// TODO: revisit documentation

			/// <summary>
			/// Adds gRPC acknowledgement duration
			/// </summary>
			/// <param name="duration">The duration</param>
			/// <returns>The builder</returns>
			public Builder WaitForAcknowledgementDuration(TimeSpan duration)
			{
				ThrowIfLessThanOneSecondOrGreaterThanThirtySeconds(duration);

				WaitForAcknowledgementDurationValue = duration;
				return this;
			}

			/// <summary>
			/// Specifies the delay between each ping to keep the gRPC connection alive
			/// </summary>
			/// <param name="duration"></param>
			/// <returns></returns>
			public Builder KeepAlivePingDelay(TimeSpan duration)
			{
				ThrowIfLessThanOneSecond(duration);

				KeepAlivePingDelayValue = duration;
				return this;
			}

			/// <summary>
			/// Specifies the timeout period we expect a ping response within
			/// </summary>
			/// <param name="duration"></param>
			/// <returns></returns>
			public Builder KeepAlivePingTimeout(TimeSpan duration)
			{
				ThrowIfLessThanOneSecond(duration);

				KeepAlivePingTimeoutValue = duration;
				return this;
			}

			private static void ThrowIfLessThanOneSecond(TimeSpan duration)
			{
				if (duration < TimeSpan.FromSeconds(1) && duration != Timeout.InfiniteTimeSpan)
				{
					throw new ArgumentOutOfRangeException(nameof(duration), duration.TotalSeconds, "Value must be at least 1 second.");
				}
			}

			private static void ThrowIfLessThanOneSecondOrGreaterThanThirtySeconds(TimeSpan duration)
			{
				if (duration < TimeSpan.FromSeconds(1) || duration > TimeSpan.FromSeconds(30))
				{
					throw new ArgumentOutOfRangeException(nameof(duration), duration.TotalSeconds, "Value must be between 1 and 30 seconds.");
				}
			}

			/// <summary>
			/// Builds a <see cref="RtgsClientOptions"/> object.
			/// </summary>
			/// <returns>The built RtgsClientOptions</returns>
			public RtgsClientOptions Build() => new(this);
		}
	}
}
