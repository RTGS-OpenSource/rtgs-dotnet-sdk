using RTGS.IDCryptSDK.JsonSignatures.Models;

namespace RTGS.DotNetSDK.Publisher.IdCrypt.Signing;

internal interface ISignMessage
{
	Type MessageType { get; }

	Task<SignDocumentResponse> SignAsync<TMessageType>(TMessageType message, string alias);
}
