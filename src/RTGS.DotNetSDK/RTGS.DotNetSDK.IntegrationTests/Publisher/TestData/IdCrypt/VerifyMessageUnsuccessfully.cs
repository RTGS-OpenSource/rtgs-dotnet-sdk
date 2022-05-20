﻿using System.Text.Json;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

internal static class VerifyMessageUnsuccessfully
{
	public const string Path = "/api/Verify";

	public static VerifyPrivateSignatureResponse Response => new()
	{
		Verified = false
	};

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, JsonSerializer.Serialize(Response));
}