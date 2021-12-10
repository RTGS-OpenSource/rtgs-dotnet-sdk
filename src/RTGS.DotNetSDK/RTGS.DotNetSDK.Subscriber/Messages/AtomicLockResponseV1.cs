using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.Messages
{
	public record AtomicLockResponseV1
	{
		public string LckId { get; init; }
		public string LckExpry { get; init; }
		public ResponseStatusCodes StatusCode { get; init; }
		public string Message { get; init; }
		public ActiveCurrencyAndAmount DbtrAmt { get; init; }
		public decimal XchgRate { get; init; }
		public Charges7 ChrgsInf { get; init; }
		public string EndToEndId { get; init; }
	}
}
