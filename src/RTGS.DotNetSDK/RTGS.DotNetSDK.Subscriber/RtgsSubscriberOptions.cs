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
			RemoteHostAddress = builder.RemoteHostAddressValue;
		}

		/// <summary>
		/// Address of the RTGS gRPC server.
		/// </summary>
		public Uri RemoteHostAddress { get; }

		/// <summary>
		/// A builder for <see cref="RtgsSubscriberOptions"/>.
		/// </summary>
		public sealed class Builder
		{
			private Builder(Uri remoteHostAddress)
			{
				RemoteHostAddressValue = remoteHostAddress;
			}

			internal Uri RemoteHostAddressValue { get; }

			/// <summary>
			/// Creates a new instance of <see cref="Builder"/>.
			/// </summary>
			/// <param name="remoteHostAddress">Address of the RTGS gRPC server.</param>
			/// <returns><see cref="Builder"/></returns>
			public static Builder CreateNew(Uri remoteHostAddress) =>
				new(remoteHostAddress);

			/// <summary>
			/// Builds a new <see cref="RtgsSubscriberOptions"/> instance.
			/// </summary>
			/// <returns><see cref="RtgsSubscriberOptions"/></returns>
			public RtgsSubscriberOptions Build() => new(this);
		}
	}
}
