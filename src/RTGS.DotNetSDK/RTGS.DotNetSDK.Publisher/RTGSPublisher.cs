using System.Threading.Tasks;
using RTGS.Public.Payment.V2;

namespace RTGS.DotNetSDK.Publisher
{
	public class RtgsPublisher : IRtgsPublisher
	{
		private readonly Payment.PaymentClient _paymentClient;

		public RtgsPublisher(Payment.PaymentClient paymentClient)
		{
			_paymentClient = paymentClient;
		}

		public async Task Wip()
		{
			using var call = _paymentClient.ToRtgsMessage();

			await call.RequestStream.WriteAsync(new RtgsMessage());
		}
	}
}
