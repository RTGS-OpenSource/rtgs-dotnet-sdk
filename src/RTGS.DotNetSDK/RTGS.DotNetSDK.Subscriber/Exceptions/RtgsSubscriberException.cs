namespace RTGS.DotNetSDK.Subscriber.Exceptions;

public class RtgsSubscriberException : Exception
{
	public RtgsSubscriberException()
	{
	}

	public RtgsSubscriberException(string message)
		: base(message)
	{
	}

	public RtgsSubscriberException(string message, string messageIdentifier)
		: this(message)
	{
		MessageIdentifier = messageIdentifier;
	}

	public RtgsSubscriberException(string message, Exception inner)
		: base(message, inner)
	{
	}

	public string MessageIdentifier { get; }
}
