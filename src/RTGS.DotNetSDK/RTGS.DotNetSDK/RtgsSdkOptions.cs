namespace RTGS.DotNetSDK;

/// <summary>
/// Represents the options used when sending messages to and receiving messages from RTGS via <see cref="IRtgsPublisher"/> and <see cref="IRtgsSubscriber"/>.
/// </summary>
public record RtgsSdkOptions
{
	private RtgsSdkOptions(Builder builder)
	{
		RtgsGlobalId = builder.RtgsGlobalIdValue;
		RemoteHostAddress = builder.RemoteHostAddressValue;
		WaitForAcknowledgementDuration = builder.WaitForAcknowledgementDurationValue;
		KeepAlivePingDelay = builder.KeepAlivePingDelayValue;
		KeepAlivePingTimeout = builder.KeepAlivePingTimeoutValue;
		IdCryptServiceAddress = builder.IdCryptServiceAddress;
		UseMessageSigning = builder.UseMessageSigningValue;
	}

	/// <summary>
	/// Organisation's unique identifier on the RTGS.global network.
	/// </summary>
	public string RtgsGlobalId { get; }

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
	/// Address of the ID Crypt Service.
	/// </summary>
	public Uri IdCryptServiceAddress { get; }

	/// <summary>
	/// Whether to use the message signing functionality (preview).
	/// </summary>
	public bool UseMessageSigning { get; }

	/// <summary>
	/// A builder for <see cref="RtgsSdkOptions"/>.
	/// </summary>
	public sealed class Builder
	{
		private Builder(
			string rtgsGlobalId,
			Uri remoteHostAddress,
			Uri idCryptServiceAddress)
		{
			ArgumentNullException.ThrowIfNull(rtgsGlobalId, nameof(rtgsGlobalId));

			if (string.IsNullOrWhiteSpace(rtgsGlobalId))
			{
				throw new ArgumentException("Value cannot be white space.", nameof(rtgsGlobalId));
			}

			ArgumentNullException.ThrowIfNull(remoteHostAddress, nameof(remoteHostAddress));

			ArgumentNullException.ThrowIfNull(idCryptServiceAddress, nameof(idCryptServiceAddress));

			RtgsGlobalIdValue = rtgsGlobalId;
			RemoteHostAddressValue = remoteHostAddress;
			IdCryptServiceAddress = idCryptServiceAddress;
		}

		internal string RtgsGlobalIdValue { get; }
		internal Uri RemoteHostAddressValue { get; }
		internal TimeSpan WaitForAcknowledgementDurationValue { get; private set; } = TimeSpan.FromSeconds(10);
		internal TimeSpan KeepAlivePingDelayValue { get; private set; } = TimeSpan.FromSeconds(30);
		internal TimeSpan KeepAlivePingTimeoutValue { get; private set; } = TimeSpan.FromSeconds(30);
		internal Uri IdCryptServiceAddress { get; }
		internal bool UseMessageSigningValue { get; private set; }

		/// <summary>
		/// Creates a new instance of <see cref="Builder"/>.
		/// </summary>
		/// <param name="rtgsGlobalId">Organisation's unique identifier on the RTGS.global network.</param>
		/// <param name="remoteHostAddress">Address of the RTGS gRPC server.</param>
		/// <param name="idCryptServiceAddress">Address of the ID Crypt Service</param>
		/// <returns><see cref="Builder"/></returns>
		/// <exception cref="ArgumentNullException">Thrown if rtgsGlobalId, remoteHostAddress idCryptApiAddress, idCryptApiKey or idCryptEndpointAddress is null.</exception>
		/// <exception cref="ArgumentException">Thrown if rtgsGlobalId or idCryptApiKey is white space.</exception>
		public static Builder CreateNew(
			string rtgsGlobalId,
			Uri remoteHostAddress,
			Uri idCryptServiceAddress) =>
			new(rtgsGlobalId, remoteHostAddress, idCryptServiceAddress);

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

		/// <summary>
		/// Enables message signing (preview).
		/// </summary>
		/// <returns><see cref="Builder"/></returns>
		public Builder EnableMessageSigning()
		{
			UseMessageSigningValue = true;
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
		/// Builds a new <see cref="RtgsSdkOptions"/> instance.
		/// </summary>
		/// <returns><see cref="RtgsSdkOptions"/></returns>
		public RtgsSdkOptions Build() => new(this);
	}
}
