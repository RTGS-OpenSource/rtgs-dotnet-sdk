using System;

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
			RemoteHostAddress = new Uri(builder.RemoteHostAddressValue);
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
			internal string BankDidValue { get; private set; }
			internal string RemoteHostAddressValue { get; private set; }
			internal TimeSpan WaitForAcknowledgementDurationValue { get; private set; } = TimeSpan.FromSeconds(10);
			internal TimeSpan KeepAlivePingDelayValue { get; private set; } = TimeSpan.FromSeconds(30);
			internal TimeSpan KeepAlivePingTimeoutValue { get; private set; } = TimeSpan.FromSeconds(30);

			/// <summary>
			/// Creates a new builder
			/// </summary>
			/// <returns>The builder</returns>
			public static Builder CreateNew() => new();

			/// <summary>
			/// Adds bank Id
			/// </summary>
			/// <param name="bankDid">The bank id</param>
			/// <returns>The builder</returns>
			public Builder BankDid(string bankDid)
			{
				BankDidValue = bankDid;
				return this;
			}

			/// <summary>
			/// Adds remote host
			/// </summary>
			/// <param name="address">The host address</param>
			/// <returns>The builder</returns>
			public Builder RemoteHost(string address)
			{
				RemoteHostAddressValue = address;
				return this;
			}

			/// <summary>
			/// Adds gRPC acknowledgement duration
			/// </summary>
			/// <param name="duration">The duration</param>
			/// <returns>The builder</returns>
			public Builder WaitForAcknowledgementDuration(TimeSpan duration)
			{
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
				KeepAlivePingTimeoutValue = duration;
				return this;
			}

			/// <summary>
			/// Builds a <see cref="RtgsClientOptions"/> object.
			/// </summary>
			/// <returns>The built RtgsClientOptions</returns>
			public RtgsClientOptions Build() => new(this);
		}
	}
}
