using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RTGS.DotNetSDK.Publisher.Extensions;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.Public.Payment.V1.Pacs;
using Xunit;
using Xunit.Abstractions;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class GivenOpenConnection : IAsyncLifetime
	{
		private readonly GrpcTestServer _server;
		private readonly IRtgsPublisher _rtgsPublisher;
		private readonly IHost _clientHost;
		private readonly IToRtgsReceiver _receiver;

		public GivenOpenConnection(ITestOutputHelper outputHelper)
		{
			_server = new GrpcTestServer(outputHelper);

			var address = _server.Start();

			var rtgsClientOptions = RtgsClientOptions.Builder.CreateNew()
				.BankDid("test-bank-did")
				.RemoteHost(address.ToString())
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureServices((_, services) => services.AddRtgsPublisher(rtgsClientOptions))
				.Build();

			_rtgsPublisher = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
			_receiver = _server.Services.GetRequiredService<IToRtgsReceiver>();
		}

		public Task InitializeAsync() => Task.CompletedTask;

		[Fact]
		public async Task ThenCanSendAtomicLockRequestToRtgs()
		{
			var atomicLockRequest = new AtomicLockRequest
			{
				DbtrToRtgsId = new GenericFinancialIdentification1
				{
					Id = nameof(ThenCanSendAtomicLockRequestToRtgs)
				},
				CdtrAmt = new ActiveCurrencyAndAmount
				{
					Ccy = "GBP",
					Amt = new ProtoDecimal
					{
						Units = 1,
						Nanos = 230_000_000
					}
				},
				UltmtDbtrAcct = new CashAccount38
				{
					Ccy = "USD",
					Id = new AccountIdentification4Choice { IBAN = "XX00ULTIMATEDEBTORACCOUNT" }
				},
				UltmtCdtrAcct = new CashAccount38
				{
					Ccy = "GBP",
					Id = new AccountIdentification4Choice { IBAN = "XX00ULTIMATECREDITORACCOUNT" }
				},
				SplmtryData = "some-extra-data",
				EndToEndId = "end-to-end-id"
			};

			await _rtgsPublisher.SendAtomicLockRequestAsync(atomicLockRequest);

			// TODO: Ditch this when SendAtomicLockRequestAsync returns a acknowledgement
			SpinWait.SpinUntil(() => _receiver.HasRequests);

			var receivedMessage = _receiver.Requests.Single();
			var receivedAtomicLockRequest = JsonConvert.DeserializeObject<AtomicLockRequest>(receivedMessage.Data);

			using var _ = new AssertionScope();

			receivedMessage.Header.Should().NotBeNull();
			receivedMessage.Header?.InstructionType.Should().Be("payment.lock.v1");
			receivedMessage.Header?.CorrelationId.Should().NotBeNullOrEmpty();

			receivedAtomicLockRequest.Should().BeEquivalentTo(atomicLockRequest, options => options.ComparingByMembers<AtomicLockRequest>());
		}

		public async Task DisposeAsync()
		{
			await _rtgsPublisher.DisposeAsync();
			_server?.Dispose();
			_clientHost?.Dispose();
		}
	}
}
