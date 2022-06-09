using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.IdCrypt;
using RTGS.DotNetSDK.Subscriber.Exceptions;
using RTGS.DotNetSDK.Subscriber.InternalMessages;

namespace RTGS.DotNetSDK.Subscriber.IdCrypt.Verification;

internal class PartnerBankEarmarkFundsV1MessageVerifier : IVerifyMessage<PartnerBankEarmarkFundsV1>
{
	private readonly IIdCryptServiceClient _idCryptServiceClient;
	private readonly ILogger<PartnerBankEarmarkFundsV1MessageVerifier> _logger;

	public PartnerBankEarmarkFundsV1MessageVerifier(IIdCryptServiceClient idCryptServiceClient, ILogger<PartnerBankEarmarkFundsV1MessageVerifier> logger)
	{
		_idCryptServiceClient = idCryptServiceClient;
		_logger = logger;
	}

	public async Task<bool> VerifyMessageAsync(
		PartnerBankEarmarkFundsV1 message,
		string privateSignature,
		string alias,
		string fromRtgsGlobalId,
		CancellationToken cancellationToken = default)
	{
		var messageToVerify = new Dictionary<string, object>
		{
			{ "creditorAmount", message.CdtrAmt },
			{ "debtorAgentAccountIban", message.DbtrAgntAcct?.Id?.IBAN },
			{ "debtorAccountIban", message.DbtrAcct?.Id?.IBAN }
		};

		try
		{
			var response = await _idCryptServiceClient.VerifyMessageAsync(fromRtgsGlobalId, messageToVerify, privateSignature, alias, cancellationToken);
			return response.Verified;
		}
		catch (Exception innerException)
		{
			const string errorMessage = "Error occurred when sending VerifyMessage request to ID Crypt Service";

			var exception = new RtgsSubscriberException(errorMessage, innerException);

			_logger.LogError(exception, errorMessage);

			throw exception;
		}
	}
}
