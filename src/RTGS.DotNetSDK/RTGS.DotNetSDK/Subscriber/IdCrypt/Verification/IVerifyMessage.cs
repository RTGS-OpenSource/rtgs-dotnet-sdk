using RTGS.Public.Payment.V4;

namespace RTGS.DotNetSDK.Subscriber.IdCrypt.Verification;

internal interface IVerifyMessage
{
	string MessageIdentifier { get; }

	Task VerifyMessageAsync(RtgsMessage rtgsMessage, CancellationToken cancellationToken = default);
}
