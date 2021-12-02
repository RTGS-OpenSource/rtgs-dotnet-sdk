﻿using System;
using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RTGS.DotNetSDK.Publisher.Extensions;
using RTGS.DotNetSDK.Publisher.IntegrationTests.TestData;
using RTGS.DotNetSDK.Publisher.Messages;
using Serilog;
using Xunit;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class GivenWrongRemoteHostAddress
	{
		[Fact]
		public async void WhenSending_ThenRpcExceptionThrown()
		{
			var rtgsClientOptions = RtgsClientOptions.Builder.CreateNew(ValidRequests.BankDid, new Uri("https://localhost:4567"))
				.Build();

			using var clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices((_, services) => services.AddRtgsPublisher(rtgsClientOptions))
				.UseSerilog()
				.Build();

			await using var rtgsPublisher = clientHost.Services.GetRequiredService<IRtgsPublisher>();

			await FluentActions.Awaiting(() => rtgsPublisher.SendAtomicLockRequestAsync(new AtomicLockRequest()))
				.Should().ThrowAsync<RpcException>();
		}
	}
}
