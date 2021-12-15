namespace RTGS.DotNetSDK.Publisher.Messages
{
	/// <summary>
	/// Used to represent the message sent to RTGS to indicate that the funds have been earmarked (or not if <see cref="Success"/> = <code>false</code>)
	/// </summary>
	public record EarmarkConfirmationV1
	{
		/// <summary>
		/// The lock Id for which to initiate earmark confirmation.
		/// </summary>
		public Guid LockId { get; init; }

		/// <summary>
		/// True to confirm, false otherwise.
		/// </summary>
		public bool Success { get; init; }
	}
}
