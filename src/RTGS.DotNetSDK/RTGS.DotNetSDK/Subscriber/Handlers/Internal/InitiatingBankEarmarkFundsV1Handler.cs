using RTGS.DotNetSDK.Subscriber.InternalMessages;
using RTGS.Public.Messages.Subscriber;

namespace RTGS.DotNetSDK.Subscriber.Handlers.Internal;

internal class InitiatingBankEarmarkFundsV1Handler : IInitiatingBankEarmarkFundsV1Handler
{
	private IHandler<EarmarkFundsV1> _userHandler;

	public void SetUserHandler(IHandler<EarmarkFundsV1> userHandler) => _userHandler = userHandler;

	public async Task HandleMessageAsync(InitiatingBankEarmarkFundsV1 message) =>
		await _userHandler.HandleMessageAsync(new EarmarkFundsV1
		{
			Acct = message.DbtrAcct,
			Amt = message.DbtrAmt,
			LckId = message.LckId
		});
}
