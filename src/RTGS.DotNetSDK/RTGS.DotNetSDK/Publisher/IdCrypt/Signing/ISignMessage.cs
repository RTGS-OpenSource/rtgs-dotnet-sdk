using RTGS.IDCrypt.Service.Contracts.SignMessage;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal interface ISignMessage<in TMessageType>
{
	Task<SignMessageResponse> SignAsync(string toRtgsGlobalId, TMessageType message, CancellationToken cancellationToken = default);
}
