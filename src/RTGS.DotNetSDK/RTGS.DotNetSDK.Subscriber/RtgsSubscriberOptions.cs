using System;

namespace RTGS.DotNetSDK.Subscriber
{
	/// <summary>
	/// Represents the options used when sending messages to RTGS via a <see cref="IRtgsSubscriber"/>.
	/// </summary>
	public class RtgsSubscriberOptions
	{
		private RtgsSubscriberOptions(Builder builder)
		{
			BankDid = builder.BankDidValue;
			RemoteHostAddress = builder.RemoteHostAddressValue;
		}

		/// <summary>
		/// Decentralized identifier of the bank.
		/// </summary>
		public string BankDid { get; }

		/// <summary>
		/// Address of the RTGS gRPC server.
		/// </summary>
		public Uri RemoteHostAddress { get; }

		/// <summary>
		/// A builder for <see cref="RtgsSubscriberOptions"/>.
		/// </summary>
		public sealed class Builder
		{
			private Builder(string bankDid, Uri remoteHostAddress)
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

			/// <summary>
			/// Creates a new instance of <see cref="Builder"/>.
			/// </summary>
			/// <param name="bankDid">Decentralized identifier of the bank.</param>
			/// <param name="remoteHostAddress">Address of the RTGS gRPC server.</param>
			/// <returns><see cref="Builder"/></returns>
			public static Builder CreateNew(string bankDid, Uri remoteHostAddress) =>
				new(bankDid, remoteHostAddress);

			/// <summary>
			/// Builds a new <see cref="RtgsSubscriberOptions"/> instance.
			/// </summary>
			/// <returns><see cref="RtgsSubscriberOptions"/></returns>
			public RtgsSubscriberOptions Build() => new(this);
		}
	}
}
