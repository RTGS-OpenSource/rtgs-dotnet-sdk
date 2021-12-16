namespace RTGS.DotNetSDK.Subscriber;

public sealed class ExceptionEventArgs : EventArgs
{
	public ExceptionEventArgs(Exception exception)
	{
		Exception = exception;
	}

	public Exception Exception { get; }
}
