using System.Runtime.Serialization;

namespace RTGS.DotNetSDK.Publisher.Exceptions;

/// <summary>
/// Represents RTGS specific exceptions.
/// </summary>
[Serializable]
public class RtgsPublisherException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="RtgsPublisherException"/> class.
	/// </summary>
	public RtgsPublisherException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RtgsPublisherException"/> class with a specific error message.
	/// </summary>
	/// <param name="message">The error message.</param>
	public RtgsPublisherException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RtgsPublisherException"/> class with a specific error message
	/// and a reference to the inner exception that is the cause of this exception.
	/// </summary>
	/// <param name="message">The error message.</param>
	/// <param name="inner">The inner exception.</param>
	public RtgsPublisherException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected RtgsPublisherException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
