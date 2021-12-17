namespace RTGS.DotNetSDK.Subscriber;

/// <summary>
/// Represents the options used when sending messages to RTGS via a <see cref="IRtgsSubscriber"/>.
/// </summary>
public class RtgsSubscriberOptions
{
	private RtgsSubscriberOptions(Builder builder)
	{
		BankDid = builder.BankDidValue;
		RemoteHostAddress = builder.RemoteHostAddressValue;
		KeepAlivePingDelay = builder.KeepAlivePingDelayValue;
		KeepAlivePingTimeout = builder.KeepAlivePingTimeoutValue;
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
	/// The delay between each ping to keep the gRPC connection alive (default 30 seconds).
	/// </summary>
	public TimeSpan KeepAlivePingDelay { get; }

	/// <summary>
	/// The timeout period within which we expect a ping response (default 30 seconds).
	/// </summary>
	public TimeSpan KeepAlivePingTimeout { get; }

	/// <summary>
	/// A builder for <see cref="RtgsSubscriberOptions"/>.
	/// </summary>
	public sealed class Builder
	{
		private Builder(string bankDid, Uri remoteHostAddress)
		{
			ArgumentNullException.ThrowIfNull(bankDid, nameof(bankDid));

			if (string.IsNullOrWhiteSpace(bankDid))
			{
				throw new ArgumentException("Value cannot be white space.", nameof(bankDid));
			}

			ArgumentNullException.ThrowIfNull(remoteHostAddress, nameof(remoteHostAddress));

			BankDidValue = bankDid;
			RemoteHostAddressValue = remoteHostAddress;
		}

		internal string BankDidValue { get; }
		internal Uri RemoteHostAddressValue { get; }
		internal TimeSpan KeepAlivePingDelayValue { get; private set; } = TimeSpan.FromSeconds(30);
		internal TimeSpan KeepAlivePingTimeoutValue { get; private set; } = TimeSpan.FromSeconds(30);

		/// <summary>
		/// Creates a new instance of <see cref="Builder"/>.
		/// </summary>
		/// <param name="bankDid">Decentralized identifier of the bank.</param>
		/// <param name="remoteHostAddress">Address of the RTGS gRPC server.</param>
		/// <returns><see cref="Builder"/></returns>
		public static Builder CreateNew(string bankDid, Uri remoteHostAddress) =>
			new(bankDid, remoteHostAddress);

		/// <summary>
		/// Specifies the delay between each ping to keep the gRPC connection alive.
		/// </summary>
		/// <param name="duration">The duration.</param>
		/// <returns><see cref="Builder"/></returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if duration is less than 1 second.</exception>
		public Builder KeepAlivePingDelay(TimeSpan duration)
		{
			ThrowIfLessThanOneSecond(duration);

			KeepAlivePingDelayValue = duration;
			return this;
		}

		/// <summary>
		/// Specifies the timeout period within which we expect a ping response.
		/// </summary>
		/// <param name="duration">The duration.</param>
		/// <returns><see cref="Builder"/></returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if duration is less than 1 second.</exception>
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

		/// <summary>
		/// Builds a new <see cref="RtgsSubscriberOptions"/> instance.
		/// </summary>
		/// <returns><see cref="RtgsSubscriberOptions"/></returns>
		public RtgsSubscriberOptions Build() => new(this);
	}
}
