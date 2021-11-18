﻿using System;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public static class ValidRequests
	{
		public const string BankDid = "test-bank-did";

		public static readonly AtomicLockRequest AtomicLockRequest = new()
		{
			DbtrToRtgsId = new Public.Payment.V1.Pacs.GenericFinancialIdentification1
			{
				Id = BankDid
			},
			CdtrAmt = new Public.Payment.V1.Pacs.ActiveCurrencyAndAmount
			{
				Ccy = "GBP",
				Amt = new ProtoDecimal
				{
					Units = 1,
					Nanos = 230_000_000
				}
			},
			UltmtDbtrAcct = new CashAccount38
			{
				Ccy = "USD",
				Id = new Public.Payment.V1.Pacs.AccountIdentification4Choice { IBAN = "XX00ULTIMATEDEBTORACCOUNT" }
			},
			UltmtCdtrAcct = new CashAccount38
			{
				Ccy = "GBP",
				Id = new Public.Payment.V1.Pacs.AccountIdentification4Choice { IBAN = "XX00ULTIMATECREDITORACCOUNT" }
			},
			SplmtryData = "some-extra-data",
			EndToEndId = "end-to-end-id"
		};

		public static readonly AtomicTransferRequest AtomicTransferRequest = new()
		{
			DbtrToRtgsId = new Public.Payment.V1.Pacs.GenericFinancialIdentification1
			{
				Id = BankDid
			},
			FIToFICstmrCdtTrf = new FinancialInstitutionToFinancialInstitutionCustomerCreditTransfer()
			{
				GrpHdr = new GroupHeader93
				{
					MsgId = "message-id"
				},
				CdtTrfTxInf =
					{
						{
							new CreditTransferTransaction39 { PoolgAdjstmntDt = "2021-01-01" }
						}
					}
			},
			LckId = "B27C2536-27F8-403F-ABBD-7AC4190FBBD3"
		};

		public static readonly EarmarkConfirmation EarmarkConfirmation = new()
		{
			LockId = new Guid("159C6010-82CB-4775-8C87-05E6EC203E8E"),
			Success = true
		};

		public static readonly TransferConfirmation TransferConfirmation = new()
		{
			LockId = new Guid("B30E15E3-CD54-4FA6-B0EB-B9BAE32976F9"),
			Success = true
		};

		public static readonly UpdateLedgerRequest UpdateLedgerRequest = new()
		{
			Amt = new ProtoDecimal()
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
	}
}