namespace RTGS.DotNetSDK.Subscriber.Messages
{
	public enum ResponseStatusCodes
	{
		NotSet = 0,
		Ok = 200,
		BadRequest = 400,
		UnprocessableEntity = 422,
		UnknownError = 1000,
		InsufficientFunds = 1001,
		AccountNotFound = 1002,
		LockNotFound = 1003,
		LockAlreadyBlocked = 1004,
		ImmutableDataNotFound = 1005,
		MissingPreferredBankPartner = 1006,
		ExchangeRateNotFound = 1007,
		CreateLockTimeout = 1008,
		BankIntegrationIssue = 1009,
		LocalBankNotOnline = 1010,
		ForeignBankNotOnline = 1011,
		AmountsDoNotMatch = 1012,
		LockNotConfirmed = 1013,
		BlockAlreadyConfirmed = 1014
	}
}
