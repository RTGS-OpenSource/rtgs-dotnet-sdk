using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RTGS.DotNetSDK.Publisher.Extensions;
using RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.Public.Payment.V1.Pacs;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class GivenOpenConnection : IAsyncLifetime
	{
		private readonly static TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

		private readonly GrpcTestServer _server;
		private IRtgsPublisher _rtgsPublisher;
		private IHost _clientHost;

		public GivenOpenConnection(ITestOutputHelper outputHelper)
		{
			try
			{
				_server = new GrpcTestServer(outputHelper);
			}
			catch (Exception)
			{
				// If an exception occurs then manually clean up the hosts
				// as IAsyncLifetime.DisposeAsync is not called.
				//DisposeHosts();

				throw;
			}
		}

		[Fact]
		public async Task ThenCanSendAtomicLockRequestToRtgs()
		{
			await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			var receiver = _server.Services.GetRequiredService<ToRtgsReceiver>();
			var receivedMessage = receiver.Requests.Should().ContainSingle().Subject;
			var receivedAtomicLockRequest = JsonConvert.DeserializeObject<AtomicLockRequest>(receivedMessage.Data);

			using var _ = new AssertionScope();

			receivedMessage.Header.Should().NotBeNull();
			receivedMessage.Header?.InstructionType.Should().Be("payment.lock.v1");
			receivedMessage.Header?.CorrelationId.Should().NotBeNullOrEmpty();

			receivedAtomicLockRequest.Should().BeEquivalentTo(ValidRequests.AtomicLockRequest, options => options.ComparingByMembers<AtomicLockRequest>());
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgement_ThenReturnTrue()
		{
			var success = await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			success.Should().BeTrue();
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsUnsuccessfulAcknowledgement_ThenReturnFalse()
		{
			var messageHandler = _server.Services.GetRequiredService<ToRtgsMessageHandler>();
			messageHandler.ReturnAcknowledgementWithFailure();

			var success = await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			success.Should().BeFalse();
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgementTooLate_ThenReturnFalse()
		{
			var messageHandler = _server.Services.GetRequiredService<ToRtgsMessageHandler>();
			messageHandler.ReturnAcknowledgementTooLate(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1)));

			var success = await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			success.Should().BeFalse();
		}

		public async Task InitializeAsync()
		{
			var address = await _server.StartAsync();

			var rtgsClientOptions = RtgsClientOptions.Builder.CreateNew()
				.BankDid("test-bank-did")
				.RemoteHost(address.ToString())
				.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureServices((_, services) => services.AddRtgsPublisher(rtgsClientOptions))
				.Build();

			_rtgsPublisher = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
		}

		public async Task DisposeAsync()
		{
			await _rtgsPublisher.DisposeAsync();
			DisposeHosts();
		}

		private void DisposeHosts()
		{
			_server?.Dispose();
			_clientHost?.Dispose();
		}

		private static class ValidRequests
		{
			public static readonly AtomicLockRequest AtomicLockRequest = new()
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
		}
	}
}
