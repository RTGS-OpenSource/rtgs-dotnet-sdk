using RTGS.Public.Payment.V1.Pacs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Publisher.Messages
{
	public class UpdateLedgerRequest
	{
		public string IBAN { get; init; }

		public GenericFinancialIdentification1 BkToRtgsId { get; init; }

		public ProtoDecimal Amt { get; init; }
	}
}
