using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.IDCrypt.Service.Contracts.Message.Verify;

namespace RTGS.DotNetSDK.IdCrypt;

internal class IdCryptServiceClient : IIdCryptServiceClient
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<IdCryptServiceClient> _logger;

	public IdCryptServiceClient(HttpClient httpClient, ILogger<IdCryptServiceClient> logger)
	{
		_httpClient = httpClient;
		_logger = logger;
	}

	public async Task<CreateConnectionInvitationResponse> CreateConnectionInvitationForRtgsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogDebug("Sending create connection invitation for RTGS request to ID Crypt Service");

			var response = await _httpClient.PostAsync("api/Connection/for-rtgs", null, cancellationToken);

			response.EnsureSuccessStatusCode();

			var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

			var createConnectionInvitationResponse =
				await JsonSerializer.DeserializeAsync<CreateConnectionInvitationResponse>(responseStream, cancellationToken: cancellationToken);

			_logger.LogDebug("Sent create connection invitation for RTGS request to ID Crypt Service");

			return createConnectionInvitationResponse;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred when sending create connection invitation for RTGS request to ID Crypt Service");

			throw;
		}
	}

	public async Task<CreateConnectionInvitationResponse> CreateConnectionInvitationForBankAsync(string toRtgsGlobalId, CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogDebug("Sending create connection invitation for bank request to ID Crypt Service");

			var request = new CreateConnectionInvitationForBankRequest
			{
				RtgsGlobalId = toRtgsGlobalId
			};

			var response = await _httpClient.PostAsJsonAsync("api/Connection/for-bank", request, cancellationToken);

			response.EnsureSuccessStatusCode();

			var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

			var createConnectionInvitationResponse =
				await JsonSerializer.DeserializeAsync<CreateConnectionInvitationResponse>(responseStream, cancellationToken: cancellationToken);

			_logger.LogDebug("Sent create connection invitation for bank request to ID Crypt Service");

			return createConnectionInvitationResponse;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred when sending create connection invitation for bank request to ID Crypt Service");

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

	public async Task<SignMessageResponse> SignMessageForBankAsync<T>(string toRtgsGlobalId, T message, CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogDebug("Sending SignMessageForBank request to ID Crypt Service");

			var document = JsonSerializer.SerializeToElement(message);

			var request = new SignMessageForBankRequest
			{
				RtgsGlobalId = toRtgsGlobalId,
				Message = document
			};

			var response = await _httpClient.PostAsJsonAsync("api/message/sign/for-bank", request, cancellationToken);

			response.EnsureSuccessStatusCode();

			var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

			var signMessageResponse = await JsonSerializer.DeserializeAsync<SignMessageResponse>(responseStream, cancellationToken: cancellationToken);

			_logger.LogDebug("Sent SignMessageForBank request to ID Crypt Service");

			return signMessageResponse;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred when sending SignMessageForBank request to ID Crypt Service");

			throw;
		}
	}

	public async Task<VerifyResponse> VerifyMessageAsync<T>(
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

			var request = new VerifyRequest
			{
				RtgsGlobalId = rtgsGlobalId,
				Message = document,
				PrivateSignature = privateSignature,
				Alias = alias
			};

			var response = await _httpClient.PostAsJsonAsync("api/message/verify", request, cancellationToken);

			response.EnsureSuccessStatusCode();

			var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

			var verifyPrivateSignatureResponse =
				await JsonSerializer.DeserializeAsync<VerifyResponse>(responseStream, cancellationToken: cancellationToken);

			_logger.LogDebug("Sent VerifyMessage request to ID Crypt Service");

			return verifyPrivateSignatureResponse;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred when sending VerifyMessage request to ID Crypt Service");

			throw;
		}
	}

	public async Task<VerifyOwnMessageResponse> VerifyOwnMessageAsync<T>(T message, string publicSignature, CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogDebug("Sending VerifyOwnMessageRequest request to ID Crypt Service");

			var document = JsonSerializer.SerializeToElement(message);

			var request = new VerifyOwnMessageRequest
			{
				Message = document,
				PublicSignature = publicSignature
			};

			var response = await _httpClient.PostAsJsonAsync("api/message/verify/own", request, cancellationToken);

			response.EnsureSuccessStatusCode();

			var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

			var verifyResponse =
				await JsonSerializer.DeserializeAsync<VerifyOwnMessageResponse>(responseStream, cancellationToken: cancellationToken);

			_logger.LogDebug("Sent VerifyOwnMessageRequest request to ID Crypt Service");

			return verifyResponse;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred when sending VerifyOwnMessageRequest request to ID Crypt Service");

			throw;
		}
	}
}
