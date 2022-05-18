using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Contracts.SignMessage;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;

namespace RTGS.DotNetSDK.IdCrypt;

internal interface IIdCryptServiceClient
{
	public Task<CreateConnectionInvitationResponse> CreateConnectionAsync(CancellationToken cancellationToken = default);
	public Task AcceptConnectionAsync(AcceptConnectionInvitationRequest request, CancellationToken cancellationToken = default);
	public Task<SignMessageResponse> SignMessageAsync<T>(string rtgsGlobalId, T message, CancellationToken cancellationToken = default);
	public Task<VerifyPrivateSignatureResponse> VerifyMessageAsync(VerifyPrivateSignatureRequest request, CancellationToken cancellationToken = default);
}
