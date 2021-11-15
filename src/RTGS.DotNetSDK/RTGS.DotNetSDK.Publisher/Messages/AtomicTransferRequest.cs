using RTGS.Public.Payment.V1.Pacs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	public record AtomicTransferRequest
	{
		public GenericFinancialIdentification1 DbtrToRtgsId { get; init; }
		public FinancialInstitutionToFinancialInstitutionCustomerCreditTransfer FIToFICstmrCdtTrf { get; init; }
		public string LckId { get; init; }
	}
}
