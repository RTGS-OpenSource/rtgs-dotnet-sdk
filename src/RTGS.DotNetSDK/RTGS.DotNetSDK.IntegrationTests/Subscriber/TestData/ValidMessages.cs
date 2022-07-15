using RTGS.DotNetSDK.Publisher.IdCrypt.Messages;
using RTGS.DotNetSDK.Subscriber.InternalMessages;
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using AccountIdentification4Choice = RTGS.ISO20022.Messages.Camt_054_001.V09.AccountIdentification4Choice;
using ActiveOrHistoricCurrencyAndAmount = RTGS.ISO20022.Messages.Camt_054_001.V09.ActiveOrHistoricCurrencyAndAmount;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.TestData;

public static class ValidMessages
{
	internal const string RtgsGlobalId = "test-bank-rtgs-global-id";

	internal static readonly PayawayFundsV1 PayawayFunds = new()
	{
		FIToFICstmrCdtTrf = new FIToFICustomerCreditTransferV10
		{
			GrpHdr = new GroupHeader96 {MsgId = "message-id"},
			CdtTrfTxInf = new[]
			{
				new CreditTransferTransaction50
				{
					PoolgAdjstmntDt = new DateTime(2021, 2, 2)
				}
			}
		}
	};

	internal static readonly FIToFICustomerCreditTransferV10 PaywayFundsVerifiable = new()
	{
		GrpHdr = new GroupHeader96 {MsgId = "message-id"},
		CdtTrfTxInf = new[]
		{
			new CreditTransferTransaction50
			{
				PoolgAdjstmntDt = new DateTime(2021, 2, 2)
			}
		}
	};

	internal static readonly PayawayCompleteV1 PayawayComplete = new()
	{
		BkToCstmrDbtCdtNtfctn = new BankToCustomerDebitCreditNotificationV09
		{
			GrpHdr = new GroupHeader81 {MsgId = "message-id"},
			Ntfctn = new[]
			{
				new AccountNotification19
				{
					Acct =
						new CashAccount41 {Id = new AccountIdentification4Choice {IBAN = "iban"}},
					Ntry = new[]
					{
						new ReportEntry11
						{
							Amt = new ActiveOrHistoricCurrencyAndAmount {Value = 5.99m},
							NtryDtls = new[]
							{
								new EntryDetails10
								{
									TxDtls = new[]
									{
										new EntryTransaction11
										{
											Refs = new TransactionReferences6
											{
												EndToEndId = "end-to-end-id"
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	};

	internal static readonly Dictionary<string, object> PayawayCompleteVerifiable = new()
	{
		{ "payawayId", "end-to-end-id"},
		{ "iban", "iban" },
		{ "amount", 5.99m }
	};

	internal static readonly MessageRejectV1 MessageRejected = new()
	{
		MsgRjctn = new ISO20022.Messages.Admi_002_001.V01.Admi00200101
		{
			RltdRef = new ISO20022.Messages.Admi_002_001.V01.MessageReference {Ref = "reference"},
			Rsn = new ISO20022.Messages.Admi_002_001.V01.RejectionReason2
			{
				RjctnDtTm = new DateTime(2021, 12, 25), RjctgPtyRsn = "Not in the right head-space"
			}
		}
	};

	internal static readonly Dictionary<string, object> MessageRejectedVerifiable = new()
	{
		{ "ref", "reference" },
		{ "reason",  "Not in the right head-space" }
	};

	internal static readonly AtomicLockResponseV1 AtomicLockResponseV1 = new()
	{
		LckId = Guid.Parse("9e4d8f43-eb2e-4408-9461-0aba281792af"),
		DbtrAmt = new ISO20022.Messages.Pacs_008_001.V10.ActiveCurrencyAndAmount {Ccy = "GBP", Value = 1.99m}
	};

	internal static readonly AtomicTransferResponseV1 AtomicTransferResponseV1 = new()
	{
		LckId = Guid.Parse("30fc2ac5-5f4d-4abc-b5b9-038df91b9832"),
		StsCd = ResponseStatusCodes.Ok,
		Msg = "the-message"
	};

	internal static readonly AtomicTransferFundsV1 AtomicTransferFundsV1 = new()
	{
		FIToFICstmrCdtTrf = new FIToFICustomerCreditTransferV10(),
		LckId = new Guid("6051b46f-a930-40fd-80ee-a08570900c87")
	};

	internal static readonly EarmarkFundsV1 EarmarkFundsV1 = new()
	{
		Amt = new ISO20022.Messages.Pacs_008_001.V10.ActiveCurrencyAndAmount {Value = 1},
		Acct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Nm = "name",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice {IBAN = "iban"}
		},
		LckId = new Guid("ff1bee59-92ac-4183-939f-6c67e16f22fb")
	};

	internal static readonly PartnerBankEarmarkFundsV1 PartnerBankEarmarkFundsV1 = new()
	{
		CdtrAmt = new ISO20022.Messages.Pacs_008_001.V10.ActiveCurrencyAndAmount {Value = 1},
		DbtrAgntAcct =
			new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
			{
				Nm = "name",
				Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice {IBAN = "iban"}
			},
		DbtrAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Nm = "name",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice {IBAN = "iban"}
		},
		LckId = new Guid("ff1bee59-92ac-4183-939f-6c67e16f22fb")
	};

	internal static readonly InitiatingBankEarmarkFundsV1 InitiatingBankEarmarkFundsV1 = new()
	{
		DbtrAmt = new ISO20022.Messages.Pacs_008_001.V10.ActiveCurrencyAndAmount {Value = 1},
		CdtrAmt = new ISO20022.Messages.Pacs_008_001.V10.ActiveCurrencyAndAmount {Value = 1},
		DbtrAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Nm = "name",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice {IBAN = "iban"}
		},
		DbtrAgntAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Nm = "name",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice {IBAN = "iban"}
		},
		LckId = new Guid("ff1bee59-92ac-4183-939f-6c67e16f22fb")
	};

	internal static readonly EarmarkCompleteV1 EarmarkCompleteV1 = new()
	{
		LckId = new Guid("4584e888-bce6-41de-b100-8ca553ad097c")
	};

	internal static readonly EarmarkReleaseV1 EarmarkReleaseV1 = new()
	{
		LckId = new Guid("19968ca5-d019-4019-9849-9f8002a3b06b")
	};

	internal static readonly BankPartnersResponseV1 BankPartnersResponseV1 = new()
	{
		DbtrAcct =
			new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
			{
				Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice {IBAN = "iban1"}
			},
		BkPrtnrs = new List<BankPartnersResponseV1.BankPartner>
		{
			new()
			{
				RtgsGlobalId =
					new ISO20022.Messages.Pacs_008_001.V10.GenericFinancialIdentification1 {Id = "id1"},
				Ccy = "PLN",
				Nm = "Bank",
				CdtrAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
				{
					Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice
					{
						IBAN = "CdtrAcctIban"
					}
				},
				CdtrAgtAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
				{
					Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice
					{
						IBAN = "CdtrAgtAcctIban"
					}
				},
				DbtrAgtAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
				{
					Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice
					{
						IBAN = "DbtrAgtAcctIban"
					}
				}
			}
		}
	};

	internal static readonly IdCryptCreateInvitationRequestV1 IdCryptCreateInvitationRequestV1 = new()
	{
		BankPartnerRtgsGlobalId = "RTGS:GB177550GB"
	};

	internal static readonly IdCryptBankInvitationV1 IdCryptBankInvitationV1 = new()
	{
		FromRtgsGlobalId = "RTGS:GB239104GB",
		Invitation = new IdCryptInvitationV1
		{
			Alias = "385ba215-7d4e-4cdc-a7a7-f14955741e70",
			Label = "the-label",
			RecipientKeys = new[] {"df3d191f-3b15-4e16-a021-09579bbbc642"},
			Id = "b705d4b8-0ef3-4ba6-8857-b3456f4ed63f",
			Type = "the-type",
			ServiceEndpoint = "https://the-service-endpoint",
			AgentPublicDid = "df3d191f-3b15-4e16-a021-09579bbbc642"
		}
	};

	internal static readonly AtomicLockApproveV2 AtomicLockApproveV2IBAN = new()
	{
		LckId = Guid.NewGuid(),
		CdtrAmt = new ISO20022.Messages.Pacs_008_001.V10.ActiveCurrencyAndAmount {Ccy = "GBP", Value = 1.23m},
		DbtrAmt = new ISO20022.Messages.Pacs_008_001.V10.ActiveCurrencyAndAmount {Ccy = "GBP", Value = 1.23m},
		DbtrAcct =
			new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
			{
				Ccy = "USD",
				Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice
				{
					IBAN = "XX00DEBTORACCOUNT"
				}
			},
		DbtrAgntAcct =
			new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
			{
				Ccy = "GBP",
				Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice
				{
					IBAN = "XX00DEBTORAGENTACCOUNT"
				}
			},
		CdtrAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "GBP",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice {IBAN = "XX00CREDITORACCOUNT"}
		},
		CdtrAgntAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "USD",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice
			{
				IBAN = "XX00CREDITORAGENTACCOUNT"
			}
		}
	};

	internal static readonly Dictionary<string, object> AtomicLockApproveV2IBANVerifiable = new()
	{
		{"creditorAmount", 1.23m},
		{"debtorAgentAccountIban", "XX00DEBTORAGENTACCOUNT"},
		{"debtorAgentAccountOtherId", null},
		{"debtorAccountIban", "XX00DEBTORACCOUNT"},
		{"debtorAccountOtherId", null},
		{"creditorAccountIban", "XX00CREDITORACCOUNT"},
		{"creditorAccountOtherId", null},
		{"creditorAgentAccountIban", "XX00CREDITORAGENTACCOUNT"},
		{"creditorAgentAccountOtherId", null}
	};

	internal static readonly AtomicLockApproveV2 AtomicLockApproveV2OtherId = new()
	{
		LckId = Guid.NewGuid(),
		CdtrAmt = new ISO20022.Messages.Pacs_008_001.V10.ActiveCurrencyAndAmount {Ccy = "GBP", Value = 1.23m},
		DbtrAmt = new ISO20022.Messages.Pacs_008_001.V10.ActiveCurrencyAndAmount {Ccy = "GBP", Value = 1.23m},
		DbtrAcct =
			new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
			{
				Ccy = "USD",
				Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice
				{
					Othr = new ISO20022.Messages.Pacs_008_001.V10.GenericAccountIdentification1
					{
						Id = "AAAA-BB-CC-123"
					}
				}
			},
		DbtrAgntAcct =
			new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
			{
				Ccy = "GBP",
				Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice
				{
					Othr = new ISO20022.Messages.Pacs_008_001.V10.GenericAccountIdentification1
					{
						Id = "BBBB-BB-CC-123"
					}
				}
			},
		CdtrAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "GBP",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice
			{
				Othr = new ISO20022.Messages.Pacs_008_001.V10.GenericAccountIdentification1
				{
					Id = "CCCC-BB-CC-123"
				}
			}
		},
		CdtrAgntAcct = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Ccy = "USD",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice
			{
				Othr = new ISO20022.Messages.Pacs_008_001.V10.GenericAccountIdentification1
				{
					Id = "DDDD-BB-CC-123"
				}
			}
		}
	};

	internal static readonly Dictionary<string, object> AtomicLockApproveV2OtherIdVerifiable = new()
	{
		{"creditorAmount", 1.23m},
		{"debtorAgentAccountIban", null},
		{"debtorAgentAccountOtherId", "BBBB-BB-CC-123"},
		{"debtorAccountIban", null},
		{"debtorAccountOtherId", "AAAA-BB-CC-123"},
		{"creditorAccountIban", null},
		{"creditorAccountOtherId", "CCCC-BB-CC-123"},
		{"creditorAgentAccountIban", null},
		{"creditorAgentAccountOtherId", "DDDD-BB-CC-123"}
	};
}
