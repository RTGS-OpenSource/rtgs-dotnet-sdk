using IDCryptGlobal.Cloud.Agent.Identity;

namespace RTGS.DotNetSDK.Publisher;

/// <summary>
/// Represents the options used when sending messages to RTGS via a <see cref="IRtgsPublisher"/>.
/// </summary>
public record RtgsPublisherOptions
{
	private RtgsPublisherOptions(Builder builder)
	{
		BankDid = builder.BankDidValue;
		RemoteHostAddress = builder.RemoteHostAddressValue;
		WaitForAcknowledgementDuration = builder.WaitForAcknowledgementDurationValue;
		KeepAlivePingDelay = builder.KeepAlivePingDelayValue;
		KeepAlivePingTimeout = builder.KeepAlivePingTimeoutValue;
		IdentityConfig = builder.IdentityConfig;
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
	/// The acknowledgement timeout duration (default 10 seconds).
	/// If the time taken to receive an acknowledgement from RTGS exceeds this duration, the request is assumed to have timed out.
	/// </summary>
	public TimeSpan WaitForAcknowledgementDuration { get; }

	/// <summary>
	/// The delay between each ping to keep the gRPC connection alive (default 30 seconds).
	/// </summary>
	public TimeSpan KeepAlivePingDelay { get; }

	/// <summary>
	/// The timeout period within which we expect a ping response (default 30 seconds).
	/// </summary>
	public TimeSpan KeepAlivePingTimeout { get; }

	/// <summary>
	/// 
	/// </summary>
	public IdentityConfig IdentityConfig { get;  }

	/// <summary>
	/// A builder for <see cref="RtgsPublisherOptions"/>.
	/// </summary>
	public sealed class Builder
	{
		private Builder(
			string bankDid, 
			Uri remoteHostAddress,
			string idCryptApiKey,
			string idCryptApiUrl,
			string idCryptServiceEndPoint)
		{
			ArgumentNullException.ThrowIfNull(bankDid, nameof(bankDid));

			if (string.IsNullOrWhiteSpace(bankDid))
			{
				throw new ArgumentException("Value cannot be white space.", nameof(bankDid));
			}

			ArgumentNullException.ThrowIfNull(remoteHostAddress, nameof(remoteHostAddress));

			BankDidValue = bankDid;
			RemoteHostAddressValue = remoteHostAddress;
			IdentityConfig = new IdentityConfig
			{
				Apikey = idCryptApiKey,
				ApiUrl = idCryptApiUrl,
				ServiceEndPoint = idCryptServiceEndPoint
			};
		}

		internal string BankDidValue { get; }
		internal Uri RemoteHostAddressValue { get; }
		internal TimeSpan WaitForAcknowledgementDurationValue { get; private set; } = TimeSpan.FromSeconds(10);
		internal TimeSpan KeepAlivePingDelayValue { get; private set; } = TimeSpan.FromSeconds(30);
		internal TimeSpan KeepAlivePingTimeoutValue { get; private set; } = TimeSpan.FromSeconds(30);

		internal IdentityConfig IdentityConfig { get; }

		/// <summary>
		/// Creates a new instance of <see cref="Builder"/>.
		/// </summary>
		/// <param name="bankDid">Decentralized identifier of the bank.</param>
		/// <param name="remoteHostAddress">Address of the RTGS gRPC server.</param>
		/// <returns><see cref="Builder"/></returns>
		/// <exception cref="ArgumentNullException">Thrown if bankDid or remoteHostAddress is null.</exception>
		/// <exception cref="ArgumentException">Thrown if bankDid is white space.</exception>
		public static Builder CreateNew(
			string bankDid, 
			Uri remoteHostAddress,
			string idCryptApiKey,
			string idCryptApiUrl,
			string idCryptServiceEndPoint) =>
			new(bankDid, remoteHostAddress, idCryptApiKey, idCryptApiUrl, idCryptServiceEndPoint);

		/// <summary>
		/// Specifies the acknowledgement timeout duration.
		/// </summary>
		/// <param name="duration">The duration.</param>
		/// <returns><see cref="Builder"/></returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if duration is not between 1 and 30 seconds.</exception>
		public Builder WaitForAcknowledgementDuration(TimeSpan duration)
		{
			ThrowIfLessThanOneSecondOrGreaterThanThirtySeconds(duration);

			WaitForAcknowledgementDurationValue = duration;
			return this;
		}

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

		private static void ThrowIfLessThanOneSecondOrGreaterThanThirtySeconds(TimeSpan duration)
		{
			if (duration < TimeSpan.FromSeconds(1) || duration > TimeSpan.FromSeconds(30))
			{
				throw new ArgumentOutOfRangeException(nameof(duration), duration.TotalSeconds, "Value must be between 1 and 30 seconds.");
			}
		}

		/// <summary>
		/// Builds a new <see cref="RtgsPublisherOptions"/> instance.
		/// </summary>
		/// <returns><see cref="RtgsPublisherOptions"/></returns>
		public RtgsPublisherOptions Build() => new(this);
	}
}
