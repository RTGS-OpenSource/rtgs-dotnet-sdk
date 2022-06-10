using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.InternalMessages;

internal record InitiatingBankEarmarkFundsV1
{
	public Guid LckId { get; init; }

	public CashAccount40 DbtrAgntAcct { get; init; }

	public CashAccount40 DbtrAcct { get; init; }

	public ActiveCurrencyAndAmount CdtrAmt { get; init; }

	public ActiveCurrencyAndAmount DbtrAmt { get; init; }
}
