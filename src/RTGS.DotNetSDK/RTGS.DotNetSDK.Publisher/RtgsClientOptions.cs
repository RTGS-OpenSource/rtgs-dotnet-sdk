using System;

namespace RTGS.DotNetSDK.Publisher
{
	/// <summary>
	/// The RtgsClientOptions class
	/// </summary>
	public record RtgsClientOptions
	{
		private RtgsClientOptions(Builder builder)
		{
			BankDid = builder.BankDidValue;
			RemoteHostAddress = new Uri(builder.RemoteHostAddressValue);
			WaitForAcknowledgementDuration = builder.WaitForAcknowledgementDurationValue;
		}

		/// <summary>
		/// The bank id
		/// </summary>
		public string BankDid { get; }
		/// <summary>
		/// Grpc server endpoint
		/// </summary>
		public Uri RemoteHostAddress { get; }
		/// <summary>
		/// Acknowledgement timeout duration
		/// </summary>
		public TimeSpan WaitForAcknowledgementDuration { get; }

		/// <summary>
		/// The Builder class
		/// </summary>
		public sealed class Builder
		{
			internal string BankDidValue { get; private set; }
			internal string RemoteHostAddressValue { get; private set; }
			internal TimeSpan WaitForAcknowledgementDurationValue { get; private set; } = TimeSpan.FromSeconds(10);

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
			/// Adds acknowledgement duration
			/// </summary>
			/// <param name="duration">The duration</param>
			/// <returns>The builder</returns>
			public Builder WaitForAcknowledgementDuration(TimeSpan duration)
			{
				WaitForAcknowledgementDurationValue = duration;
				return this;
			}

			/// <summary>
			/// Builds a client options object.
			/// </summary>
			/// <returns>The builder</returns>
			public RtgsClientOptions Build() => new RtgsClientOptions(this);
		}
	}
}
