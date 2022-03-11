using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.Messages;

public record BankPartnersResponseV1
{
	public IEnumerable<BankPartner> BkPtnrs { get; init; }

	public CashAccount40 DbtrAcct { get; init; }

	public record BankPartner
	{
		public string Ccy { get; init; }
		public string BkNm { get; init; }
		public GenericFinancialIdentification1 Id { get; init; }
		public CashAccount40 DbtrAgtAcct { get; init; }
		public CashAccount40 CdtrAcct { get; init; }
		public CashAccount40 CdtrAgtAcct { get; init; }
	}
}
