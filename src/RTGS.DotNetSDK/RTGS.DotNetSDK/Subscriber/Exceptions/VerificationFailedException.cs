using System.Runtime.Serialization;

namespace RTGS.DotNetSDK.Subscriber.Exceptions;

/// <summary>
/// Represents a failure in ID Crypt verification..
/// </summary>
[Serializable]
public class VerificationFailedException : RtgsSubscriberException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="VerificationFailedException"/> class.
	/// </summary>
	public VerificationFailedException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="VerificationFailedException"/> class with a specific error message.
	/// </summary>
	/// <param name="message">The error message.</param>
	public VerificationFailedException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="VerificationFailedException"/> class with a specific error message and message identifier.
	/// </summary>
	/// <param name="message">The error message.</param>
	/// <param name="messageIdentifier">The message identifier.</param>
	public VerificationFailedException(string message, string messageIdentifier)
		: base(message, messageIdentifier)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="VerificationFailedException"/> class with a specific error message
	/// and a reference to the inner exception that is the cause of this exception.
	/// </summary>
	/// <param name="message">The error message.</param>
	/// <param name="inner">The inner exception.</param>
	public VerificationFailedException(string message, Exception inner)
		: base(message, inner)
	{
	}
	protected VerificationFailedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
