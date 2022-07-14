using Microsoft.Extensions.Logging;
using RTGS.DotNetSDK.IdCrypt;
using RTGS.DotNetSDK.Subscriber.Exceptions;

namespace RTGS.DotNetSDK.Subscriber.IdCrypt.Verification;

internal class AtomicLockApproveV2MessageVerifier : IVerifyMessage<AtomicLockApproveV2>
{
	private readonly IIdCryptServiceClient _idCryptServiceClient;
	private readonly ILogger<PayawayFundsV1MessageVerifier> _logger;
	
	public AtomicLockApproveV2MessageVerifier(IIdCryptServiceClient idCryptServiceClient, ILogger<PayawayFundsV1MessageVerifier> logger)
	{
		_idCryptServiceClient = idCryptServiceClient;
		_logger = logger;
	}
	
	public async Task<bool> VerifyMessageAsync(
		AtomicLockApproveV2 message,
		string privateSignature,
		string publicSignature,
		string alias,
		string fromRtgsGlobalId,
		CancellationToken cancellationToken = default)
	{
		var messageToVerify = new Dictionary<string, object>
		{
			{"creditorAmount", message.CdtrAmt},
			{"debtorAgentAccountIban", message.DbtrAgntAcct?.Id?.IBAN},
			{"debtorAgentAccountOtherId", message.DbtrAgntAcct?.Id?.Othr?.Id},
			{"debtorAccountIban", message.DbtrAcct?.Id?.IBAN},
			{"debtorAccountOtherId", message.DbtrAcct?.Id?.Othr?.Id},
			{"creditorAccountIban", message.CdtrAcct?.Id?.IBAN},
			{"creditorAccountOtherId", message.CdtrAcct?.Id?.Othr?.Id},
			{"creditorAgentAccountIban", message.CdtrAgntAcct?.Id?.IBAN},
			{"creditorAgentAccountOtherId", message.CdtrAgntAcct?.Id?.Othr?.Id}
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
