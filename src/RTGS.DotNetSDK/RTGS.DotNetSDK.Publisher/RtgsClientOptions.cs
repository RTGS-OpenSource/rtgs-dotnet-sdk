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
		}

		public string BankDid { get; }
		public Uri RemoteHostAddress { get; }
		public TimeSpan WaitForAcknowledgementDuration { get; }

		public sealed class Builder
		{
			internal string BankDidValue { get; private set; }
			internal string RemoteHostAddressValue { get; private set; }
			internal TimeSpan WaitForAcknowledgementDurationValue { get; private set; } = TimeSpan.FromSeconds(10);

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

			public RtgsClientOptions Build()
			{
				return new RtgsClientOptions(this);
			}
		}
	}
}
