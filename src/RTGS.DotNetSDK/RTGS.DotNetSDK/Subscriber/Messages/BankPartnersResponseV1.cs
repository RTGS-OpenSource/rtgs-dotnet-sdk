using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Subscriber.Messages;

public record BankPartnersResponseV1
{
	/// <summary>
	/// BankPartners: List of bank partners.
	/// </summary>
	public IEnumerable<BankPartner> BkPrtnrs { get; init; }

	/// <summary>
	/// DebtorAccount: Debtor account details.
	/// </summary>
	/// <remarks>
	/// The <c>CashAccount40</c> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
	/// </remarks>
	public CashAccount40 DbtrAcct { get; init; }

	public record BankPartner
	{
		/// <summary>
		/// Currency: bank partner currency.
		/// </summary>
		public string Ccy { get; init; }

		/// <summary>
		/// Name: bank name.
		/// </summary>
		public string Nm { get; init; }

		/// <summary>
		/// RtgsGlobalId: Bank partner's unique identifier on the RTGS.global network.
		/// </summary>
		/// <remarks>
		/// The <c>GenericFinancialIdentification1</c> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
		/// </remarks>
		public GenericFinancialIdentification1 RtgsGlobalId { get; init; }

		/// <summary>
		/// DebtorAgentAccount: Debtor agent account details.
		/// </summary>
		/// <remarks>
		/// The <c>CashAccount40</c> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
		/// </remarks>
		public CashAccount40 DbtrAgtAcct { get; init; }

		/// <summary>
		/// CreditorAccount: Creditor account details.
		/// </summary>
		/// <remarks>
		/// The <c>CashAccount40</c> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
		/// </remarks>
		public CashAccount40 CdtrAcct { get; init; }

		/// <summary>
		/// CreditorAgentAccount: Creditor agent account details.
		/// </summary>
		/// <remarks>
		/// The <c>CashAccount40</c> type is from NuGet package RTGS.ISO20022.Messages <see href="https://www.nuget.org/packages/RTGS.ISO20022.Messages/"/>
		/// </remarks>
		public CashAccount40 CdtrAgtAcct { get; init; }
	}
}
