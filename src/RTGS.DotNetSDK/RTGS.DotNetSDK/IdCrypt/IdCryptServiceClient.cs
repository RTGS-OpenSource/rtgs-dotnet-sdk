using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Contracts.SignMessage;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;

namespace RTGS.DotNetSDK.IdCrypt;

internal class IdCryptServiceClient : IIdCryptServiceClient
{
	private readonly HttpClient _httpClient;
	private readonly RtgsSdkOptions _options;
	private readonly ILogger<IdCryptServiceClient> _logger;


	public IdCryptServiceClient(HttpClient httpClient, RtgsSdkOptions options, ILogger<IdCryptServiceClient> logger)
	{
		_httpClient = httpClient;
		_options = options;
		_logger = logger;
	}

	public async Task<CreateConnectionInvitationResponse> CreateConnectionInvitationAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogDebug("Sending CreateConnectionInvitation request to ID Crypt Service");

			var response = await _httpClient.PostAsync("api/Connection", null, cancellationToken);

			response.EnsureSuccessStatusCode();

			var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

			var createConnectionInvitationResponse =
				await JsonSerializer.DeserializeAsync<CreateConnectionInvitationResponse>(responseStream, cancellationToken: cancellationToken);

			_logger.LogDebug("Sent CreateConnectionInvitation request to ID Crypt Service");

			return createConnectionInvitationResponse;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred when sending CreateConnectionInvitation request to ID Crypt Service");

			throw;
		}
	}

	public async Task AcceptConnectionInvitationAsync(AcceptConnectionInvitationRequest request, CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogDebug("Sending AcceptConnectionInvitation request to ID Crypt Service");

			var response = await _httpClient.PostAsJsonAsync("api/Connection/Accept", request, cancellationToken);

			response.EnsureSuccessStatusCode();

			_logger.LogDebug("Sent AcceptConnectionInvitation request to ID Crypt Service");
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred when sending AcceptConnectionInvitation request to ID Crypt Service");

			throw;
		}
	}

	public async Task<SignMessageResponse> SignMessageAsync<T>(T message, CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogDebug("Sending SignMessage request to ID Crypt Service");

			var document = JsonSerializer.SerializeToElement(message);

			var request = new SignMessageRequest
			{
				RtgsGlobalId = _options.RtgsGlobalId,
				Message = document
			};

			var response = await _httpClient.PostAsJsonAsync("api/SignMessage", request, cancellationToken);

			response.EnsureSuccessStatusCode();

			var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

			var signMessageResponse = await JsonSerializer.DeserializeAsync<SignMessageResponse>(responseStream, cancellationToken: cancellationToken);

			_logger.LogDebug("Sent SignMessage request to ID Crypt Service");

			return signMessageResponse;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred when sending SignMessage request to ID Crypt Service");

			throw;
		}
	}

	public async Task<VerifyPrivateSignatureResponse> VerifyMessageAsync<T>(
		string rtgsGlobalId,
		T message,
		string privateSignature,
		string alias,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogDebug("Sending VerifyMessage request to ID Crypt Service");

			var document = JsonSerializer.SerializeToElement(message);

			var request = new VerifyPrivateSignatureRequest
			{
				RtgsGlobalId = rtgsGlobalId,
				Message = document,
				PrivateSignature = privateSignature,
				Alias = alias
			};

			var response = await _httpClient.PostAsJsonAsync("api/Verify", request, cancellationToken);

			response.EnsureSuccessStatusCode();

			var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

			var verifyPrivateSignatureResponse =
				await JsonSerializer.DeserializeAsync<VerifyPrivateSignatureResponse>(responseStream, cancellationToken: cancellationToken);

			_logger.LogDebug("Sent VerifyMessage request to ID Crypt Service");

			return verifyPrivateSignatureResponse;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred when sending VerifyMessage request to ID Crypt Service");

			throw;
		}
	}
}
