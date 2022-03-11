namespace RTGS.DotNetSDK.Subscriber.Exceptions;

/// <summary>
/// Represents RTGS specific exceptions.
/// </summary>
public class RtgsSubscriberException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="RtgsSubscriberException"/> class.
	/// </summary>
	public RtgsSubscriberException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RtgsSubscriberException"/> class with a specific error message.
	/// </summary>
	/// <param name="message">The error message.</param>
	public RtgsSubscriberException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RtgsSubscriberException"/> class with a specific error message and message identifier.
	/// </summary>
	/// <param name="message">The error message.</param>
	/// <param name="messageIdentifier">The message identifier.</param>
	public RtgsSubscriberException(string message, string messageIdentifier)
		: this(message)
	{
		MessageIdentifier = messageIdentifier;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RtgsSubscriberException"/> class with a specific error message
	/// and a reference to the inner exception that is the cause of this exception.
	/// </summary>
	/// <param name="message">The error message.</param>
	/// <param name="inner">The inner exception.</param>
	public RtgsSubscriberException(string message, Exception inner)
		: base(message, inner)
	{
	}

	/// <summary>
	/// Gets the message identifier.
	/// </summary>
	public string MessageIdentifier { get; }
}
