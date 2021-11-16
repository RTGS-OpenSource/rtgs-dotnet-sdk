﻿using RTGS.Public.Payment.V1.Pacs;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	public record AtomicTransferRequest
	{
		public GenericFinancialIdentification1 DbtrToRtgsId { get; init; }
		public Public.Payment.V1.Pacs.FinancialInstitutionToFinancialInstitutionCustomerCreditTransfer FIToFICstmrCdtTrf { get; init; }
		public string LckId { get; init; }
	}
}