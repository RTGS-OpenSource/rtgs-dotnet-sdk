using RTGS.Public.Payment.V3;

namespace RTGS.DotNetSDK.Subscriber.IdCrypt.Verification;

internal interface IVerifyMessage
{
	string MessageIdentifier { get; }

	Task VerifyMessageAsync(RtgsMessage rtgsMessage, CancellationToken cancellationToken = default);
}
