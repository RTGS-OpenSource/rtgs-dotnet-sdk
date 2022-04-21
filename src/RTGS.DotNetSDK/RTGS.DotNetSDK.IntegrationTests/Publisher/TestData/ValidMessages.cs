using RTGS.ISO20022.Messages.Admi_002_001.V01;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public static class ValidMessages
{
	public const string RtgsGlobalId = "test-bank-rtgs-global-id";
	public const string IdCryptAlias = "id-crypt-alias";

	public static readonly AtomicLockRequestV1 AtomicLockRequest = new()
	{
		BkPrtnrRtgsGlobalId = null,
		CdtrAmt = new ISO20022.Messages.Pacs_008_001.V10.ActiveCurrencyAndAmount
		{
			Ccy = "GBP",
			Value = 1.23m
		},
		UltmtDbtrAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "USD",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice { IBAN = "XX00ULTIMATEDEBTORACCOUNT" }
		},
		UltmtCdtrAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "GBP",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice { IBAN = "XX00ULTIMATECREDITORACCOUNT" }
		},
		DbtrAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "USD",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice { IBAN = "XX00DEBTORACCOUNT" }
		},
		DbtrAgntAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "GBP",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice { IBAN = "XX00DEBTORAGENTACCOUNT" }
		},
		CdtrAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "GBP",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice { IBAN = "XX00CREDITORACCOUNT" }
		},
		CdtrAgntAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "USD",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice { IBAN = "XX00CREDITORAGENTACCOUNT" }
		},
		EndToEndId = "end-to-end-id"
	};

	public static readonly AtomicLockRequestV1 AtomicLockRequestWithBankPartnerRtgsGlobalId = new()
	{
		BkPrtnrRtgsGlobalId = "RTGS:GB12345GBP",
		CdtrAmt = new ISO20022.Messages.Pacs_008_001.V10.ActiveCurrencyAndAmount
		{
			Ccy = "GBP",
			Value = 1.23m
		},
		UltmtDbtrAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "USD",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice { IBAN = "XX00ULTIMATEDEBTORACCOUNT" }
		},
		UltmtCdtrAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "GBP",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice { IBAN = "XX00ULTIMATECREDITORACCOUNT" }
		},
		DbtrAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "USD",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice { IBAN = "XX00DEBTORACCOUNT" }
		},
		DbtrAgntAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "GBP",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice { IBAN = "XX00DEBTORAGENTACCOUNT" }
		},
		CdtrAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "GBP",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice { IBAN = "XX00CREDITORACCOUNT" }
		},
		CdtrAgntAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "USD",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice { IBAN = "XX00CREDITORAGENTACCOUNT" }
		},
		EndToEndId = "end-to-end-id"
	};

	public static readonly AtomicTransferRequestV1 AtomicTransferRequest = new()
	{
		FIToFICstmrCdtTrf = new FIToFICustomerCreditTransferV10
		{
			GrpHdr = new GroupHeader96 { MsgId = "message-id" },
			CdtTrfTxInf = new[]
			{
				new CreditTransferTransaction50 { PoolgAdjstmntDt = DateTime.Parse("2021-01-01") }
			}
		},
		LckId = Guid.Parse("B27C2536-27F8-403F-ABBD-7AC4190FBBD3")
	};

	public static readonly EarmarkConfirmationV1 EarmarkConfirmation = new()
	{
		LckId = new Guid("159C6010-82CB-4775-8C87-05E6EC203E8E"),
		Sccs = true
	};

	public static readonly AtomicTransferConfirmationV1 AtomicTransferConfirmation = new()
	{
		LckId = new Guid("B30E15E3-CD54-4FA6-B0EB-B9BAE32976F9"),
		Sccs = true
	};

	public static readonly UpdateLedgerRequestV1 UpdateLedgerRequest = new()
	{
		Amt = new ISO20022.Messages.Pacs_008_001.V10.ActiveCurrencyAndAmount
		{
			Value = 1.23m
		},
		AcctId = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice
		{
			IBAN = "GB33BUKB20201555555555"
		}
	};

	public static readonly FIToFICustomerCreditTransferV10 PayawayCreate = new()
	{
		GrpHdr = new GroupHeader96
		{
			MsgId = "message-id"
		},
		CdtTrfTxInf = new[]
		{
			new CreditTransferTransaction50
			{
				IntrBkSttlmAmt = new ISO20022.Messages.Pacs_008_001.V10.ActiveCurrencyAndAmount
				{
					Ccy = "jpy",
					Value = 1
				},
				PoolgAdjstmntDt = new DateTime(2021, 1, 1),
				CdtrAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
				{
					Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice
					{
						IBAN = "iban"
					}
				}
			}
		}
	};

	public static readonly BankToCustomerDebitCreditNotificationV09 PayawayConfirmation = new()
	{
		GrpHdr = new GroupHeader81
		{
			MsgId = "message-id"
		},
		Ntfctn = new[]
		{
			new AccountNotification19
			{
				AddtlNtfctnInf = "additional-notification-info"
			}
		}
	};

	public static readonly Admi00200101 PayawayRejection = new()
	{
		RltdRef = new MessageReference
		{
			Ref = "payaway-id"
		}
	};

	public static readonly BankPartnersRequestV1 BankPartnersRequest = new();
}
