namespace RTGS.DotNetSDK.Publisher;

/// <summary>
/// Represents the result of a message sent to RTGS.
/// </summary>
public enum SendResult
{
	/// <summary>
	/// The result cannot be determined.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// The message was sent successfully.
	/// </summary>
	Success = 1,

	/// <summary>
	/// The send operation exceeded the configured duration.
	/// </summary>
	Timeout = 2,

	/// <summary>
	/// The message was rejected.
	/// </summary>
	Rejected = 3,
}
