using RTGS.IDCrypt.Service.Contracts.SignMessage;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal interface ISignMessage<in TMessageType>
{
	Task<SignMessageResponse> SignAsync(TMessageType message, string partnerRtgsGlobalId, CancellationToken cancellationToken = default);
}
