namespace RTGS.DotNetSDK.Publisher
{
	/// <summary>
	/// The SendResult enum
	/// </summary>
	public enum SendResult
	{
		/// <summary>
		/// Unknown result
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Success result
		/// </summary>
		Success = 1,

		/// <summary>
		/// Timeout result
		/// </summary>
		Timeout = 2,

		/// <summary>
		/// ServerError result
		/// </summary>
		ServerError = 3,
	}
}
