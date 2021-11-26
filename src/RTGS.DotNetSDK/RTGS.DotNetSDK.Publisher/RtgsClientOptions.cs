using System;

namespace RTGS.DotNetSDK.Publisher
{
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

		public string BankDid { get; }
		public Uri RemoteHostAddress { get; }
		public TimeSpan WaitForAcknowledgementDuration { get; }

		/// <summary>
		/// The delay between each ping to keep the gRPC connection alive (default 30 seconds)
		/// </summary>
		public TimeSpan KeepAlivePingDelay { get; }

		/// <summary>
		/// The timeout period we expect a ping response within (default 30 seconds)
		/// </summary>
		public TimeSpan KeepAlivePingTimeout { get; }

		public sealed class Builder
		{
			internal string BankDidValue { get; private set; }
			internal string RemoteHostAddressValue { get; private set; }
			internal TimeSpan WaitForAcknowledgementDurationValue { get; private set; } = TimeSpan.FromSeconds(10);
			internal TimeSpan KeepAlivePingDelayValue { get; private set; } = TimeSpan.FromSeconds(30);
			internal TimeSpan KeepAlivePingTimeoutValue { get; private set; } = TimeSpan.FromSeconds(30);

			public static Builder CreateNew() => new();

			public Builder BankDid(string bankDid)
			{
				BankDidValue = bankDid;
				return this;
			}

			public Builder RemoteHost(string address)
			{
				RemoteHostAddressValue = address;
				return this;
			}

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

			public RtgsClientOptions Build()
			{
				return new RtgsClientOptions(this);
			}
		}
	}
}
