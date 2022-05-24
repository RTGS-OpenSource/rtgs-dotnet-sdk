using System.Runtime.Serialization;

namespace RTGS.DotNetSDK.Subscriber.Exceptions;

/// <summary>
/// Represents RTGS specific exceptions.
/// </summary>
[Serializable]
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

	protected RtgsSubscriberException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		MessageIdentifier = info.GetString("MessageIdentifier");
	}

	public new virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);

		info.AddValue(nameof(MessageIdentifier), MessageIdentifier, typeof(string));
	}

	/// <summary>
	/// Gets the message identifier.
	/// </summary>
	public string MessageIdentifier { get; }
}
