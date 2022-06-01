using RTGS.DotNetSDK.Subscriber.InternalMessages;
using RTGS.Public.Messages.Subscriber;

namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;

internal class PartnerBankEarmarkFundsV1Handler : IPartnerBankEarmarkFundsV1Handler
{
	private IHandler<EarmarkFundsV1> _userHandler;

	public void SetUserHandler(IHandler<EarmarkFundsV1> userHandler) => _userHandler = userHandler;

	public async Task HandleMessageAsync(PartnerBankEarmarkFundsV1 message) =>
		await _userHandler.HandleMessageAsync(new EarmarkFundsV1
		{
			LckId = message.LckId,
			Amt = message.CdtrAmt,
			Acct = message.CdtrAgntAcct
		});
}
