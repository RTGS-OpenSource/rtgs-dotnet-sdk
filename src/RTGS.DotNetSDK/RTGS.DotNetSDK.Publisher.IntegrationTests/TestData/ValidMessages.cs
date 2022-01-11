using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;

public static class ValidMessages
{
	public const string BankDid = "test-bank-did";

	public static readonly AtomicLockRequestV1 AtomicLockRequest = new()
	{
		DbtrToRtgsId = new ISO20022.Messages.Pacs_008_001.V10.GenericFinancialIdentification1
		{
			Id = BankDid
		},
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
		SplmtryData = "some-extra-data",
		EndToEndId = "end-to-end-id"
	};

	public static readonly AtomicTransferRequestV1 AtomicTransferRequest = new()
	{
		DbtrToRtgsId = new Public.Payment.V1.Pacs.GenericFinancialIdentification1
		{
			Id = BankDid
		},
		FIToFICstmrCdtTrf = new Public.Payment.V1.Pacs.FinancialInstitutionToFinancialInstitutionCustomerCreditTransfer()
		{
			GrpHdr = new Public.Payment.V1.Pacs.GroupHeader93
			{
				MsgId = "message-id"
			},
			CdtTrfTxInf =
			{
				{
					new Public.Payment.V1.Pacs.CreditTransferTransaction39 { PoolgAdjstmntDt = "2021-01-01" }
				}
			}
		},
		LckId = "B27C2536-27F8-403F-ABBD-7AC4190FBBD3"
	};

	public static readonly EarmarkConfirmationV1 EarmarkConfirmation = new()
	{
		LockId = new Guid("159C6010-82CB-4775-8C87-05E6EC203E8E"),
		Success = true
	};

	public static readonly AtomicTransferConfirmationV1 AtomicTransferConfirmation = new()
	{
		LockId = new Guid("B30E15E3-CD54-4FA6-B0EB-B9BAE32976F9"),
		Success = true
	};

	public static readonly UpdateLedgerRequestV1 UpdateLedgerRequest = new()
	{
		Amt = new Public.Payment.V1.Pacs.ProtoDecimal()
		{
			Units = 1,
			Nanos = 230_000_000
		},
		BkToRtgsId = new Public.Payment.V1.Pacs.GenericFinancialIdentification1()
		{
			Id = BankDid
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
			new CreditTransferTransaction50 { PoolgAdjstmntDt = new DateTime(2021, 1, 1) }
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

	public static readonly BankPartnersRequestV1 BankPartnersRequest = new();
}
