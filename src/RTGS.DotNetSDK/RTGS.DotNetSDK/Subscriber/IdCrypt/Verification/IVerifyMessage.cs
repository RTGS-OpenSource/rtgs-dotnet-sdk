namespace RTGS.DotNetSDK.Subscriber.IdCrypt.Verification;

internal interface IVerifyMessage<in TMessage>
{
	Task<bool> VerifyMessageAsync(
		TMessage message,
		string privateSignature,
		string alias,
		string fromRtgsGlobalId,
		CancellationToken cancellationToken = default);
}
