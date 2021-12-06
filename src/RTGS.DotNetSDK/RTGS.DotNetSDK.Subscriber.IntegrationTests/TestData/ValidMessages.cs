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
	}
}
