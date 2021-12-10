using System;

namespace RTGS.DotNetSDK.Subscriber.Exceptions
{
	// TODO: follow custom excpetions best practices
	// https://docs.microsoft.com/en-us/dotnet/standard/exceptions/how-to-create-user-defined-exceptions
	public class RtgsSubscriberException : Exception
	{
		public RtgsSubscriberException()
		{
		}

		public RtgsSubscriberException(string message) : base(message)
		{
		}

		public RtgsSubscriberException(string message, string messageIdentifier) : this(message)
		{
			MessageIdentifier = messageIdentifier;
		}

		public string MessageIdentifier { get; }
	}
}
