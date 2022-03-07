using RTGS.DotNetSDK.Subscriber.Messages;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData;

public static class ValidMessages
{
	public const string BankDid = "test-bank-did";

	public static readonly ISO20022.Messages.Pacs_008_001.V10.FIToFICustomerCreditTransferV10 PayawayFunds = new()
	{
		GrpHdr = new ISO20022.Messages.Pacs_008_001.V10.GroupHeader96
		{
			MsgId = "message-id"
		},
		CdtTrfTxInf = new[]
		{
			new ISO20022.Messages.Pacs_008_001.V10.CreditTransferTransaction50
			{
				PoolgAdjstmntDt = new DateTime(2021, 2, 2)
			}
		}
	};

	public static readonly ISO20022.Messages.Camt_054_001.V09.BankToCustomerDebitCreditNotificationV09 PayawayComplete = new()
	{
		GrpHdr = new ISO20022.Messages.Camt_054_001.V09.GroupHeader81
		{
			MsgId = "message-id"
		},
		Ntfctn = new[]
		{
			new ISO20022.Messages.Camt_054_001.V09.AccountNotification19
			{
				Ntry = new[]
				{
					new ISO20022.Messages.Camt_054_001.V09.ReportEntry11
					{
						NtryDtls = new[]
						{
							new ISO20022.Messages.Camt_054_001.V09.EntryDetails10
							{
								TxDtls = new[]
								{
									new ISO20022.Messages.Camt_054_001.V09.EntryTransaction11
									{
										Refs = new ISO20022.Messages.Camt_054_001.V09.TransactionReferences6
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
	};

	public static readonly ISO20022.Messages.Admi_002_001.V01.Admi00200101 MessageRejected = new()
	{
		RltdRef = new ISO20022.Messages.Admi_002_001.V01.MessageReference
		{
			Ref = "reference"
		},
		Rsn = new ISO20022.Messages.Admi_002_001.V01.RejectionReason2
		{
			RjctnDtTm = new DateTime(2021, 12, 25)
		}
	};

	public static readonly AtomicLockResponseV1 AtomicLockResponseV1 = new()
	{
		LckId = "9e4d8f43-eb2e-4408-9461-0aba281792af",
		DbtrAmt = new ISO20022.Messages.Pacs_008_001.V10.ActiveCurrencyAndAmount
		{
			Ccy = "GBP",
			Value = 1.99m
		}
	};

	public static readonly AtomicTransferResponseV1 AtomicTransferResponseV1 = new()
	{
		LckId = "30fc2ac5-5f4d-4abc-b5b9-038df91b9832",
		StatusCode = ResponseStatusCodes.Ok,
		Message = "the-message"
	};

	public static readonly AtomicTransferFundsV1 AtomicTransferFundsV1 = new()
	{
		PacsJson = "pacs-json",
		LockId = new Guid("6051b46f-a930-40fd-80ee-a08570900c87")
	};

	public static readonly EarmarkFundsV1 EarmarkFundsV1 = new()
	{
		Amount = 1,
		LiquidityPoolAccount = new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
		{
			Nm = "name",
			Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice
			{
				IBAN = "iban"
			}
		},
		LockId = new Guid("ff1bee59-92ac-4183-939f-6c67e16f22fb")
	};

	public static readonly EarmarkCompleteV1 EarmarkCompleteV1 = new()
	{
		LockId = new Guid("4584e888-bce6-41de-b100-8ca553ad097c")
	};

	public static readonly EarmarkReleaseV1 EarmarkReleaseV1 = new()
	{
		LockId = new Guid("19968ca5-d019-4019-9849-9f8002a3b06b")
	};

	public static readonly BankPartnersResponseV1 BankPartnersResponseV1 = new()
	{
		DbtrAcct =
			new ISO20022.Messages.Pacs_008_001.V10.CashAccount40()
			{
				Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice { IBAN = "iban1" }
			},
		BkPtnrs = new List<BankPartnersResponseV1.BankPartner>
		{
			new BankPartnersResponseV1.BankPartner
			{
				Id =
					new ISO20022.Messages.Pacs_008_001.V10.GenericFinancialIdentification1
					{
						Id = "id1"
					},
				Ccy = "PLN",
				BkNm = "Bank",
				CdtrAcct =
					new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
					{
						Id = new ISO20022.Messages.Pacs_008_001.V10.AccountIdentification4Choice
						{
							IBAN = "CdtrAcctIban"
						}
					},
				CdtrAgtAcct =
					new ISO20022.Messages.Pacs_008_001.V10.CashAccount40
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

	public static readonly IdCryptInvitationConfirmationV1 IdCryptInvitationConfirmationV1 = new()
	{
		ConnectionId = new Guid("1d6f914b-3f9d-4cc4-a396-f4ba7154b7ae").ToString()
	};
}
