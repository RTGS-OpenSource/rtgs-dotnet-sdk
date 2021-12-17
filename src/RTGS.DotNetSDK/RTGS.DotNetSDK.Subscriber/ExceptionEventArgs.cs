namespace RTGS.DotNetSDK.Subscriber;

public sealed class ExceptionEventArgs : EventArgs
{
	public ExceptionEventArgs(Exception exception, bool isFatal)
	{
		Exception = exception;
		IsFatal = isFatal;
	}

	public Exception Exception { get; }

	public bool IsFatal { get; }
}
