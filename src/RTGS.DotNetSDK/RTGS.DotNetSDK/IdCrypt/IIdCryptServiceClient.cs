using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.IDCrypt.Service.Contracts.Message.Verify;

namespace RTGS.DotNetSDK.IdCrypt;

internal interface IIdCryptServiceClient
{
	public Task<CreateConnectionInvitationResponse> CreateConnectionInvitationAsync(CancellationToken cancellationToken = default);
	public Task AcceptConnectionInvitationAsync(AcceptConnectionInvitationRequest request, CancellationToken cancellationToken = default);
	public Task<SignMessageResponse> SignMessageAsync<T>(string toRtgsGlobalId, T message, CancellationToken cancellationToken = default);
	public Task<VerifyResponse> VerifyMessageAsync<T>(
		string rtgsGlobalId,
		T message,
		string privateSignature,
		string alias,
		CancellationToken cancellationToken = default);
}
