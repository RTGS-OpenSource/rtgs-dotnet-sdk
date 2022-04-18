using RTGS.IDCryptSDK.JsonSignatures.Models;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal interface ISignMessage<TMessageType>
{
	Task<SignDocumentResponse> SignAsync(TMessageType message, string alias, CancellationToken cancellationToken = default);
}
