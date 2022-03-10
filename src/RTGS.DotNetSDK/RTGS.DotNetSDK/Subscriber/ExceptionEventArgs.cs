namespace RTGS.DotNetSDK.Subscriber;

/// <summary>
/// Provides data for the exception occurred event.
/// </summary>
public sealed class ExceptionEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ExceptionEventArgs"/> class.
	/// </summary>
	/// <param name="exception">The exception that has been thrown.</param>
	/// <param name="isFatal">Whether the exception is fatal.</param>
	public ExceptionEventArgs(Exception exception, bool isFatal)
	{
		Exception = exception;
		IsFatal = isFatal;
	}

	/// <summary>
	/// Gets the exception that has been thrown.
	/// </summary>
	public Exception Exception { get; }

	/// <summary>
	/// Gets whether the exception is fatal.
	/// A fatal exception means the subscriber is no longer receiving messages.
	/// </summary>
	public bool IsFatal { get; }
}
