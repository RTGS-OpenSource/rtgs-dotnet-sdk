namespace RTGS.DotNetSDK.Publisher
{
	public enum SendResult
	{
		Unknown = 0,
		Success = 1,
		Timeout = 2,
		ServerError = 3,
		ConnectionError = 4,
		ClientError = 5,
	}
}
