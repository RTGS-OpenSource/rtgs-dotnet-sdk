using System;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData
{
	public static class ValidMessages
	{
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
	}
}
